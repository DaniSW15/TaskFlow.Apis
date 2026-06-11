# TaskFlow API — Guía Flutter

> **Stack:** Flutter 3 · Dart · Dio · Riverpod · SharedPreferences

**Base URL:** `http://10.0.2.2:8080/api` (emulador Android) · `http://localhost:8080/api` (iOS/web)

---

## Instalación

```yaml
# pubspec.yaml
dependencies:
  dio: ^5.4.0
  flutter_riverpod: ^2.5.0
  shared_preferences: ^2.2.0
  go_router: ^13.0.0
```

```bash
flutter pub get
```

---

## Configuración

```dart
// lib/core/constants.dart
class AppConfig {
  static const String apiUrl = 'http://10.0.2.2:8080/api'; // Android emulator
  // static const String apiUrl = 'http://localhost:8080/api'; // iOS/web
}
```

---

## Modelos

```dart
// lib/core/models/auth_response.dart
class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final String accessTokenExpiresAt;
  final String userId;
  final String email;
  final String firstName;
  final String lastName;
  final int role; // 0=Member 1=Admin 2=Analyst 3=Client

  const AuthResponse({
    required this.accessToken, required this.refreshToken,
    required this.accessTokenExpiresAt, required this.userId,
    required this.email, required this.firstName,
    required this.lastName, required this.role,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> j) => AuthResponse(
    accessToken: j['accessToken'],
    refreshToken: j['refreshToken'],
    accessTokenExpiresAt: j['accessTokenExpiresAt'],
    userId: j['userId'],
    email: j['email'],
    firstName: j['firstName'],
    lastName: j['lastName'],
    role: j['role'],
  );
}
```

```dart
// lib/core/models/api_response.dart
class ApiResponse<T> {
  final bool success;
  final T? data;
  final String message;
  final List<String>? errors;

  const ApiResponse({
    required this.success, this.data,
    required this.message, this.errors,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> j,
    T Function(dynamic)? fromData,
  ) => ApiResponse(
    success: j['success'],
    data: j['data'] != null && fromData != null ? fromData(j['data']) : null,
    message: j['message'] ?? '',
    errors: (j['errors'] as List?)?.cast<String>(),
  );
}
```

```dart
// lib/core/models/board.dart
class Board {
  final String id, title, ownerId, createdAt, updatedAt;
  final String? description;
  final int taskCount;

  const Board({
    required this.id, required this.title, required this.ownerId,
    required this.createdAt, required this.updatedAt,
    this.description, required this.taskCount,
  });

  factory Board.fromJson(Map<String, dynamic> j) => Board(
    id: j['id'], title: j['title'], ownerId: j['ownerId'],
    createdAt: j['createdAt'], updatedAt: j['updatedAt'],
    description: j['description'], taskCount: j['taskCount'],
  );
}
```

```dart
// lib/core/models/task_item.dart
class TaskItem {
  final String id, title, status, priority, boardId, createdAt, updatedAt;
  final String? description, dueDate, assigneeId, assigneeName;

  const TaskItem({
    required this.id, required this.title, required this.status,
    required this.priority, required this.boardId,
    required this.createdAt, required this.updatedAt,
    this.description, this.dueDate, this.assigneeId, this.assigneeName,
  });

  factory TaskItem.fromJson(Map<String, dynamic> j) => TaskItem(
    id: j['id'], title: j['title'], status: j['status'],
    priority: j['priority'], boardId: j['boardId'],
    createdAt: j['createdAt'], updatedAt: j['updatedAt'],
    description: j['description'], dueDate: j['dueDate'],
    assigneeId: j['assigneeId'], assigneeName: j['assigneeName'],
  );
}
```

```dart
// lib/core/models/paginated_list.dart
class PaginatedList<T> {
  final List<T> items;
  final int totalCount, pageNumber, pageSize, totalPages;
  final bool hasNextPage, hasPreviousPage;

  const PaginatedList({
    required this.items, required this.totalCount,
    required this.pageNumber, required this.pageSize,
    required this.totalPages, required this.hasNextPage,
    required this.hasPreviousPage,
  });

  factory PaginatedList.fromJson(
    Map<String, dynamic> j,
    T Function(Map<String, dynamic>) fromItem,
  ) => PaginatedList(
    items: (j['items'] as List).map((e) => fromItem(e)).toList(),
    totalCount: j['totalCount'],
    pageNumber: j['pageNumber'],
    pageSize: j['pageSize'],
    totalPages: j['totalPages'],
    hasNextPage: j['hasNextPage'],
    hasPreviousPage: j['hasPreviousPage'],
  );
}
```

---

## Auth Storage

```dart
// lib/core/storage/auth_storage.dart
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:convert';
import '../models/auth_response.dart';

class AuthStorage {
  static const _key = 'tf_auth';

  static Future<void> save(AuthResponse auth) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_key, jsonEncode({
      'accessToken': auth.accessToken,
      'refreshToken': auth.refreshToken,
      'accessTokenExpiresAt': auth.accessTokenExpiresAt,
      'userId': auth.userId,
      'email': auth.email,
      'firstName': auth.firstName,
      'lastName': auth.lastName,
      'role': auth.role,
    }));
  }

  static Future<AuthResponse?> load() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_key);
    if (raw == null) return null;
    return AuthResponse.fromJson(jsonDecode(raw));
  }

  static Future<void> clear() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_key);
  }
}
```

---

## Cliente HTTP (Dio)

```dart
// lib/core/http/api_client.dart
import 'package:dio/dio.dart';
import '../constants.dart';
import '../storage/auth_storage.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._();
  static ApiClient get instance => _instance;

  late final Dio _dio;

  ApiClient._() {
    _dio = Dio(BaseOptions(
      baseUrl: AppConfig.apiUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {'Content-Type': 'application/json'},
    ));

    // Adjunta token
    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final auth = await AuthStorage.load();
        if (auth != null) {
          options.headers['Authorization'] = 'Bearer ${auth.accessToken}';
        }
        handler.next(options);
      },

      // Renueva token en 401
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          try {
            final auth = await AuthStorage.load();
            if (auth == null) return handler.next(error);

            final res = await Dio().post(
              '${AppConfig.apiUrl}/Auth/refresh',
              data: {
                'accessToken': auth.accessToken,
                'refreshToken': auth.refreshToken,
              },
            );

            final newAuth = AuthResponse.fromJson(res.data['data']);
            await AuthStorage.save(newAuth);

            // Reintenta la petición original
            final retried = await _dio.request(
              error.requestOptions.path,
              options: Options(
                method: error.requestOptions.method,
                headers: {'Authorization': 'Bearer ${newAuth.accessToken}'},
              ),
              data: error.requestOptions.data,
              queryParameters: error.requestOptions.queryParameters,
            );
            return handler.resolve(retried);
          } catch (_) {
            await AuthStorage.clear();
          }
        }
        handler.next(error);
      },
    ));
  }

  Dio get dio => _dio;
}
```

---

## Auth Provider (Riverpod)

```dart
// lib/features/auth/auth_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/http/api_client.dart';
import '../../core/models/auth_response.dart';
import '../../core/storage/auth_storage.dart';

final authProvider = StateNotifierProvider<AuthNotifier, AuthResponse?>(
  (ref) => AuthNotifier(),
);

class AuthNotifier extends StateNotifier<AuthResponse?> {
  AuthNotifier() : super(null) { _load(); }

  final _dio = ApiClient.instance.dio;

  Future<void> _load() async {
    state = await AuthStorage.load();
  }

  Future<void> login(String email, String password) async {
    final res = await _dio.post('/Auth/login', data: { 'email': email, 'password': password });
    final auth = AuthResponse.fromJson(res.data['data']);
    await AuthStorage.save(auth);
    state = auth;
  }

  Future<void> register(String firstName, String lastName, String email, String password) async {
    final res = await _dio.post('/Auth/register', data: {
      'firstName': firstName, 'lastName': lastName,
      'email': email, 'password': password,
    });
    final auth = AuthResponse.fromJson(res.data['data']);
    await AuthStorage.save(auth);
    state = auth;
  }

  Future<void> logout() async {
    try { await _dio.post('/Auth/logout'); } catch (_) {}
    await AuthStorage.clear();
    state = null;
  }

  bool get isAdmin => state?.role == 1;
  bool get isAdminOrAnalyst => state?.role == 1 || state?.role == 2;
}
```

---

## Services

```dart
// lib/features/boards/boards_service.dart
import 'package:dio/dio.dart';
import '../../core/http/api_client.dart';
import '../../core/models/board.dart';
import '../../core/models/paginated_list.dart';

class BoardsService {
  final _dio = ApiClient.instance.dio;

  Future<PaginatedList<Board>> getBoards({int page = 1, int pageSize = 20}) async {
    final res = await _dio.get('/Boards', queryParameters: {
      'pageNumber': page, 'pageSize': pageSize,
    });
    return PaginatedList.fromJson(res.data['data'], Board.fromJson);
  }

  Future<String> createBoard(String title, {String? description}) async {
    final res = await _dio.post('/Boards', data: { 'title': title, 'description': description });
    return res.data['data'];
  }

  Future<void> updateBoard(String id, String title, {String? description}) async {
    await _dio.put('/Boards/$id', data: { 'title': title, 'description': description });
  }

  Future<void> deleteBoard(String id) async {
    await _dio.delete('/Boards/$id');
  }
}
```

```dart
// lib/features/tasks/tasks_service.dart
import 'package:dio/dio.dart';
import '../../core/http/api_client.dart';
import '../../core/models/task_item.dart';
import '../../core/models/paginated_list.dart';

class TasksService {
  final _dio = ApiClient.instance.dio;

  Future<PaginatedList<TaskItem>> getTasks(String boardId, {int page = 1}) async {
    final res = await _dio.get('/Tasks', queryParameters: {
      'boardId': boardId, 'pageNumber': page, 'pageSize': 50,
    });
    return PaginatedList.fromJson(res.data['data'], TaskItem.fromJson);
  }

  Future<String> createTask(String boardId, Map<String, dynamic> body) async {
    final res = await _dio.post('/Tasks', queryParameters: { 'boardId': boardId }, data: body);
    return res.data['data'];
  }

  Future<void> updateTask(String id, Map<String, dynamic> body) async {
    await _dio.put('/Tasks/$id', data: body);
  }

  Future<void> deleteTask(String id) async => _dio.delete('/Tasks/$id');

  Future<void> addComment(String taskId, String content) async {
    await _dio.post('/Tasks/$taskId/comments', data: { 'content': content });
  }

  Future<void> addTag(String taskId, String tagId) async {
    await _dio.post('/Tasks/$taskId/tags/$tagId');
  }

  Future<void> removeTag(String taskId, String tagId) async {
    await _dio.delete('/Tasks/$taskId/tags/$tagId');
  }
}
```

---

## Ejemplo: pantalla de Login

```dart
// lib/features/auth/login_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'auth_provider.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});
  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _loading = false;
  String? _error;

  Future<void> _submit() async {
    setState(() { _loading = true; _error = null; });
    try {
      await ref.read(authProvider.notifier).login(_emailCtrl.text, _passwordCtrl.text);
      if (mounted) Navigator.pushReplacementNamed(context, '/boards');
    } catch (e) {
      setState(() { _error = 'Credenciales inválidas'; });
    } finally {
      setState(() { _loading = false; });
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Iniciar sesión')),
    body: Padding(
      padding: const EdgeInsets.all(16),
      child: Column(children: [
        TextField(controller: _emailCtrl, decoration: const InputDecoration(labelText: 'Email')),
        TextField(controller: _passwordCtrl, obscureText: true, decoration: const InputDecoration(labelText: 'Contraseña')),
        if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
        const SizedBox(height: 16),
        ElevatedButton(
          onPressed: _loading ? null : _submit,
          child: _loading ? const CircularProgressIndicator() : const Text('Entrar'),
        ),
      ]),
    ),
  );
}
```

---

## Ejemplo: lista de Boards

```dart
// lib/features/boards/boards_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/models/board.dart';
import '../../core/models/paginated_list.dart';
import 'boards_service.dart';

final boardsProvider = FutureProvider.autoDispose<PaginatedList<Board>>(
  (_) => BoardsService().getBoards(),
);

class BoardsScreen extends ConsumerWidget {
  const BoardsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final boards = ref.watch(boardsProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Mis Boards')),
      body: boards.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Error: $e')),
        data: (data) => ListView.builder(
          itemCount: data.items.length,
          itemBuilder: (_, i) {
            final board = data.items[i];
            return ListTile(
              title: Text(board.title),
              subtitle: Text('${board.taskCount} tareas'),
              trailing: IconButton(
                icon: const Icon(Icons.delete),
                onPressed: () async {
                  await BoardsService().deleteBoard(board.id);
                  ref.invalidate(boardsProvider);
                },
              ),
              onTap: () => Navigator.pushNamed(context, '/boards/${board.id}'),
            );
          },
        ),
      ),
      floatingActionButton: FloatingActionButton(
        child: const Icon(Icons.add),
        onPressed: () async {
          await BoardsService().createBoard('Nuevo Board');
          ref.invalidate(boardsProvider);
        },
      ),
    );
  }
}
```

---

## Manejo de errores de la API

```dart
// lib/core/http/api_exception.dart
import 'package:dio/dio.dart';

String parseApiError(DioException e) {
  final data = e.response?.data;
  if (data == null) return 'Error de conexión';
  final errors = data['errors'] as List?;
  if (errors != null && errors.isNotEmpty) return errors.join(' · ');
  return data['message'] ?? 'Error desconocido';
}
```

```dart
// Uso en cualquier servicio/pantalla
try {
  await service.createBoard('Nuevo');
} on DioException catch (e) {
  showSnackbar(context, parseApiError(e));
}
```

---

## Estructura del proyecto

```
lib/
├── core/
│   ├── constants.dart
│   ├── http/
│   │   ├── api_client.dart     Dio + interceptores
│   │   └── api_exception.dart
│   ├── models/
│   │   ├── auth_response.dart
│   │   ├── board.dart
│   │   ├── task_item.dart
│   │   ├── paginated_list.dart
│   │   └── ...
│   └── storage/
│       └── auth_storage.dart   SharedPreferences
├── features/
│   ├── auth/
│   │   ├── auth_provider.dart  Riverpod StateNotifier
│   │   ├── login_screen.dart
│   │   └── register_screen.dart
│   ├── boards/
│   │   ├── boards_service.dart
│   │   └── boards_screen.dart
│   ├── tasks/
│   │   ├── tasks_service.dart
│   │   └── tasks_screen.dart
│   └── admin/
│       └── admin_screen.dart
├── router.dart                 go_router
└── main.dart
```

---

## Tips rápidos

- URL del emulador Android: `10.0.2.2` apunta al `localhost` de tu máquina.
- `ref.invalidate(provider)` en Riverpod = refresca los datos tras una mutación.
- Los tokens se guardan en `SharedPreferences` — no en memoria, sobreviven reinicios.
- Para acceso condicional por rol: `ref.read(authProvider)?.role == 1` (Admin).
- Errores de validación de la API vienen en `response.data['errors']` como `List<String>`.
- Para infinite scroll usar `ScrollController` + detectar `atEdge` → llamar con `cursor` siguiente.
