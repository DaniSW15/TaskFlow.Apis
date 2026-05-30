# Guía de integración Angular 21 + TaskFlow API

Esta guía cubre cómo consumir la TaskFlow API desde una aplicación Angular 21 usando el nuevo sistema de signals, standalone components, functional guards e interceptors.

---

## Referencia completa de la API

> **Base URL local:** `http://localhost:8080/api`
> **Base URL producción:** `https://tu-dominio.com/api`
> **Formato:** JSON. Todos los endpoints devuelven `ApiResponse<T>`.

### Wrapper de respuesta

```json
// Éxito
{
  "success": true,
  "data": { ... },
  "message": "OK",
  "errors": null
}

// Error
{
  "success": false,
  "data": null,
  "message": "Descripción del error",
  "errors": ["Error detallado 1", "Error detallado 2"]
}
```

---

### 🔐 Auth — sin token requerido (excepto logout)

#### `POST /api/Auth/register`
```json
// Request
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan@example.com",
  "password": "Segura1234!"
}

// Response 201 — data:
{
  "accessToken": "eyJ...",
  "refreshToken": "abc123...",
  "accessTokenExpiresAt": "2026-05-27T12:15:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "juan@example.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "role": 0
}
```
> `role`: `0` = Member, `1` = Admin, `2` = Analyst, `3` = Client

---

#### `POST /api/Auth/login`
```json
// Request
{ "email": "juan@example.com", "password": "Segura1234!" }

// Response 200 — misma estructura que register
```

---

#### `POST /api/Auth/refresh`
```json
// Request
{ "accessToken": "eyJ...", "refreshToken": "abc123..." }

// Response 200 — misma estructura que login
```

---

#### `POST /api/Auth/logout` 🔒
```
Headers: Authorization: Bearer <accessToken>
Body: (vacío)
Response 200: { "success": true, "data": null, ... }
```

---

### 📋 Boards 🔒 — todos requieren token

#### `GET /api/Boards?pageNumber=1&pageSize=20`
```json
// Response 200 — data:
{
  "items": [
    {
      "id": "3fa85f64-...",
      "title": "Mi Board",
      "description": "Descripción opcional",
      "ownerId": "3fa85f64-...",
      "taskCount": 5,
      "createdAt": "2026-05-27T10:00:00Z",
      "updatedAt": "2026-05-27T10:00:00Z"
    }
  ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

#### `GET /api/Boards/{id}`
```json
// Response 200 — data: BoardDto (mismo objeto que items[])
// Response 404 si no existe o no pertenece al usuario
```

---

#### `POST /api/Boards`
```json
// Request
{ "title": "Nuevo Board", "description": "Opcional" }

// Response 201 — data: "3fa85f64-..." (Guid del nuevo board)
```

---

#### `PUT /api/Boards/{id}`
```json
// Request
{ "title": "Título actualizado", "description": "Nueva desc" }

// Response 200 — data: null
// Response 403 si no eres el dueño
// Response 404 si no existe
```

---

#### `DELETE /api/Boards/{id}`
```
// Response 200 — soft-delete
// Response 403 si no eres el dueño
```

---

### 👤 Users 🔒

#### `GET /api/Users` — solo Admin
```json
// Query params: pageNumber=1&pageSize=20
// Response 200 — data: PaginatedList<UserDto>
{
  "items": [
    {
      "id": "3fa85f64-...",
      "firstName": "Juan",
      "lastName": "Pérez",
      "fullName": "Juan Pérez",
      "email": "juan@example.com",
      "role": 0,
      "createdAt": "2026-05-27T10:00:00Z",
      "updatedAt": "2026-05-27T10:00:00Z"
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

#### `GET /api/Users/me`
```json
// Response 200 — data: UserDto del usuario autenticado
```

#### `PUT /api/Users/me`
```json
// Request — actualizar perfil propio
{ "firstName": "Juan", "lastName": "García" }
// Response 200
```

#### `GET /api/Users/{id}` — solo Admin
```json
// Response 200 — data: UserDto
```

#### `PATCH /api/Users/{id}/role` — solo Admin
```json
// Request
{ "role": 2 }
// role: 0=Member, 1=Admin, 2=Analyst, 3=Client
// Response 200
```

#### `DELETE /api/Users/{id}` — solo Admin
```
// Response 200 — soft-delete
```

---

### ✅ Tasks 🔒 — todos requieren token

#### `GET /api/Tasks?boardId={id}&pageNumber=1&pageSize=50`
```json
// Response 200 — data: PaginatedList<TaskDto>
{
  "items": [
    {
      "id": "3fa85f64-...",
      "title": "Implementar login",
      "description": "Usar JWT",
      "status": "Todo",
      "priority": "High",
      "dueDate": "2026-06-01T00:00:00Z",
      "boardId": "3fa85f64-...",
      "assigneeId": null,
      "assigneeName": null,
      "createdAt": "2026-05-27T10:00:00Z",
      "updatedAt": "2026-05-27T10:00:00Z"
    }
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

> **status:** `"Todo"` | `"InProgress"` | `"Done"` | `"Cancelled"`
> **priority:** `"Low"` | `"Medium"` | `"High"` | `"Critical"`

---

#### `GET /api/Tasks/{id}`
```json
// Response 200 — data: TaskDto (mismo objeto que items[])
```

---

#### `POST /api/Tasks?boardId={boardId}`
```json
// Request
{
  "title": "Nueva tarea",
  "description": "Opcional",
  "priority": "Medium",
  "dueDate": "2026-06-01T00:00:00Z",
  "assigneeId": null
}

// Response 201 — data: "3fa85f64-..." (Guid de la nueva tarea)
```

---

#### `PUT /api/Tasks/{id}`
```json
// Request
{
  "title": "Título actualizado",
  "description": "Opcional",
  "status": "InProgress",
  "priority": "High",
  "dueDate": "2026-06-15T00:00:00Z",
  "assigneeId": null
}

// Response 200 — data: null
// Response 403 si no tienes permiso
```

---

#### `DELETE /api/Tasks/{id}`
```
// Response 200 — soft-delete
```

---

### 💬 Comments 🔒 — sub-recurso de Task

#### `GET /api/Tasks/{taskId}/comments?pageNumber=1&pageSize=20`
```json
// Response 200 — data: PaginatedList<CommentDto>
{
  "items": [
    {
      "id": "3fa85f64-...",
      "content": "Revisar validación del token",
      "taskItemId": "3fa85f64-...",
      "authorId": "3fa85f64-...",
      "authorFullName": "Juan Pérez",
      "createdAt": "2026-05-27T10:00:00Z"
    }
  ],
  "totalCount": 3,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

#### `POST /api/Tasks/{taskId}/comments`
```json
// Request
{ "content": "Mi comentario" }
// Response 201 — data: Guid del nuevo comentario
```

#### `DELETE /api/Tasks/{taskId}/comments/{commentId}`
```
// Solo el autor puede borrar su propio comentario
// Response 200 — soft-delete
// Response 403 si no eres el autor
```

---

### 🏷️ Tags 🔒

#### `GET /api/Tags`
```json
// Response 200 — data: TagDto[]
[
  { "id": "3fa85f64-...", "name": "Bug", "color": "#EF4444" },
  { "id": "3fa85f64-...", "name": "Feature", "color": "#6366F1" }
]
```
> Resultado cacheado 10 minutos en el servidor.

#### `POST /api/Tags` — solo Admin
```json
// Request
{ "name": "Urgente", "color": "#F59E0B" }
// Response 201 — data: Guid del nuevo tag
```

#### `DELETE /api/Tags/{id}` — solo Admin
```
// Response 200
```

#### `POST /api/Tasks/{taskId}/tags/{tagId}` — asociar tag (N:M)
```
// No body. Inserta fila en tabla TaskItemTags.
// Response 200
// Idempotente: si ya tiene ese tag, responde 200 sin duplicar.
```

#### `DELETE /api/Tasks/{taskId}/tags/{tagId}` — desasociar tag
```
// No body. Elimina fila de TaskItemTags.
// Response 200
// Idempotente: si no tiene ese tag, responde 200 igual.
```

---

### 🏢 Clients 🔒

#### `GET /api/Clients?pageNumber=1&pageSize=20`
```json
// Response 200 — data: PaginatedList<ClientDto>
{
  "items": [
    {
      "id": "3fa85f64-...",
      "name": "Empresa S.A.",
      "email": "contacto@empresa.com",
      "phone": "+1234567890",
      "company": "Empresa S.A.",
      "notes": "Cliente premium",
      "projectCount": 3,
      "createdAt": "2026-05-27T10:00:00Z",
      "updatedAt": "2026-05-27T10:00:00Z"
    }
  ],
  ...
}
```

#### `GET /api/Clients/{id}`
```json
// Response 200 — data: ClientDto
```

#### `POST /api/Clients` — Admin o Analyst
```json
// Request
{
  "name": "Empresa S.A.",
  "email": "contacto@empresa.com",
  "phone": "+1234567890",
  "company": "Empresa S.A.",
  "notes": "Opcional"
}
// Response 201 — data: Guid
```

#### `PUT /api/Clients/{id}` — Admin o Analyst
```json
// Request — mismos campos que POST
// Response 200
```

#### `DELETE /api/Clients/{id}` — solo Admin
```
// Response 200 — soft-delete
```

---

### 📁 Projects 🔒

#### `GET /api/Projects?pageNumber=1&pageSize=20`
```json
// Response 200 — data: PaginatedList<ProjectDto>
{
  "items": [
    {
      "id": "3fa85f64-...",
      "title": "Web App 2026",
      "description": "Rediseño completo",
      "status": "Active",
      "startDate": "2026-01-01T00:00:00Z",
      "endDate": "2026-12-31T00:00:00Z",
      "clientId": "3fa85f64-...",
      "clientName": "Empresa S.A.",
      "analystId": "3fa85f64-...",
      "analystFullName": "Ana López",
      "boardCount": 2,
      "createdAt": "2026-05-27T10:00:00Z",
      "updatedAt": "2026-05-27T10:00:00Z"
    }
  ],
  ...
}
```
> **status:** `"Planning"` | `"Active"` | `"OnHold"` | `"Completed"` | `"Cancelled"`

#### `GET /api/Projects/{id}`
```json
// Response 200 — data: ProjectDto (incluye boards anidados)
```

#### `POST /api/Projects` — Admin o Analyst
```json
// Request
{
  "title": "Web App 2026",
  "description": "Opcional",
  "status": "Planning",
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-12-31T00:00:00Z",
  "clientId": "3fa85f64-...",
  "analystId": "3fa85f64-..."
}
// Response 201 — data: Guid
```

#### `PUT /api/Projects/{id}` — Admin o Analyst
```json
// Request — mismos campos que POST
// Response 200
```

#### `DELETE /api/Projects/{id}` — solo Admin
```
// Response 200 — soft-delete
```

---

### Códigos de error comunes

| Código | Significado |
|---|---|
| `400` | Validación fallida — ver `errors[]` |
| `401` | Token inválido o expirado |
| `403` | Sin permiso sobre ese recurso |
| `404` | Recurso no encontrado |
| `409` | Conflicto (ej. email ya registrado) |
| `429` | Rate limit excedido |
| `500` | Error interno del servidor |

---

## Requisitos previos

```bash
node -v   # 22+
ng version # Angular CLI 21
```

```bash
npm install -g @angular/cli@21
ng new taskflow-frontend --standalone --routing --style=scss
cd taskflow-frontend
```

---

## 1. Configuración base

### environment.ts

```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080/api'
};
```

```typescript
// src/environments/environment.production.ts
export const environment = {
  production: true,
  apiUrl: 'https://tu-dominio.com/api'
};
```

---

## 2. Modelos (interfaces)

```typescript
// src/app/core/models/auth.models.ts
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export enum UserRole {
  Member = 0,
  Admin = 1,
  Analyst = 2,
  Client = 3
}
```

```typescript
// src/app/core/models/board.models.ts
export interface Board {
  id: string;
  title: string;
  description?: string;
  ownerId: string;
  taskCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CursorPaginatedList<T> {
  items: T[];
  nextCursor: string | null;
  hasNextPage: boolean;
  pageSize: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[] | null;
}
```

```typescript
// src/app/core/models/comment.models.ts
export interface Comment {
  id: string;
  content: string;
  taskItemId: string;
  authorId: string;
  authorFullName: string;
  createdAt: string;
}
```

```typescript
// src/app/core/models/tag.models.ts
export interface Tag {
  id: string;
  name: string;
  color: string; // hex, ej: "#6366F1"
}
```

```typescript
// src/app/core/models/client.models.ts
export interface Client {
  id: string;
  name: string;
  email: string;
  phone?: string;
  company?: string;
  notes?: string;
  projectCount: number;
  createdAt: string;
  updatedAt: string;
}
```

```typescript
// src/app/core/models/project.models.ts
export interface Project {
  id: string;
  title: string;
  description?: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
  clientId: string;
  clientName: string;
  analystId: string;
  analystFullName: string;
  boardCount: number;
  createdAt: string;
  updatedAt: string;
}

export enum ProjectStatus {
  Planning = 'Planning',
  Active = 'Active',
  OnHold = 'OnHold',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}
```

```typescript
// src/app/core/models/user.models.ts
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
  updatedAt: string;
}
```

```typescript
// src/app/core/models/task.models.ts
export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  boardId: string;
  assigneeId?: string;
  assigneeName?: string;
  createdAt: string;
  updatedAt: string;
}

export enum TaskItemStatus {
  Todo = 'Todo',
  InProgress = 'InProgress',
  Done = 'Done',
  Cancelled = 'Cancelled'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}
```

---

## 3. AuthService con Signals

```typescript
// src/app/core/services/auth.service.ts
import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError, EMPTY } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'tf_access_token';
const REFRESH_TOKEN_KEY = 'tf_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  // --- Signals ---
  private readonly _user = signal<AuthResponse | null>(this.loadStoredUser());

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.role === UserRole.Admin);
  readonly isAnalyst = computed(() => this._user()?.role === UserRole.Analyst);
  readonly isAdminOrAnalyst = computed(() =>
    this._user()?.role === UserRole.Admin || this._user()?.role === UserRole.Analyst
  );
  readonly accessToken = computed(() => localStorage.getItem(ACCESS_TOKEN_KEY));

  // --- Auth actions ---

  register(request: RegisterRequest) {
    return this.http.post<{ data: AuthResponse }>(`${this.baseUrl}/register`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  login(request: LoginRequest) {
    return this.http.post<{ data: AuthResponse }>(`${this.baseUrl}/login`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  logout() {
    return this.http.post(`${this.baseUrl}/logout`, {}).pipe(
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return EMPTY;
      })
    );
  }

  refreshToken() {
    const rt = localStorage.getItem(REFRESH_TOKEN_KEY);
    const at = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!rt || !at) return EMPTY;

    return this.http.post<{ data: AuthResponse }>(`${this.baseUrl}/refresh`, {
      accessToken: at,
      refreshToken: rt
    }).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  // --- Internal ---

  private handleAuthSuccess(auth: AuthResponse) {
    localStorage.setItem(ACCESS_TOKEN_KEY, auth.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, auth.refreshToken);
    this._user.set(auth);
  }

  private clearSession() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  private loadStoredUser(): AuthResponse | null {
    // Reconstruye el estado desde el token almacenado si existe
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000;
      if (Date.now() > exp) return null; // token expirado

      return null; // Dejamos que el interceptor renueve; aquí solo chequeamos si hay sesión
    } catch {
      return null;
    }
  }
}
```

---

## 4. HTTP Interceptor (agrega Bearer token)

Angular 21 usa interceptores funcionales:

```typescript
// src/app/core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.accessToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Intentar renovar el token automáticamente
        return auth.refreshToken().pipe(
          switchMap(() => {
            const newToken = auth.accessToken();
            const retried = req.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` }
            });
            return next(retried);
          }),
          catchError(() => {
            router.navigate(['/login']);
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
```

Registrar en `app.config.ts`:

```typescript
// src/app/app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

---

## 5. BoardsService

```typescript
// src/app/core/services/boards.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, Board, PaginatedList } from '../models/board.models';

@Injectable({ providedIn: 'root' })
export class BoardsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Boards`;

  getBoards(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Board>>>(this.baseUrl, { params });
  }

  getBoardById(id: string) {
    return this.http.get<ApiResponse<Board>>(`${this.baseUrl}/${id}`);
  }

  createBoard(title: string, description?: string) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, { title, description });
  }

  updateBoard(id: string, title: string, description?: string) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, { title, description });
  }

  deleteBoard(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 6. TasksService

```typescript
// src/app/core/services/tasks.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/board.models';
import { TaskItem, TaskItemStatus, TaskPriority } from '../models/task.models';

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: string;
  assigneeId?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  assigneeId?: string;
}

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tasks`;

  getTasksByBoard(boardId: string, pageNumber = 1, pageSize = 50) {
    const params = new HttpParams()
      .set('boardId', boardId)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<TaskItem>>>(this.baseUrl, { params });
  }

  getTaskById(id: string) {
    return this.http.get<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`);
  }

  createTask(boardId: string, request: CreateTaskRequest) {
    const params = new HttpParams().set('boardId', boardId);
    return this.http.post<ApiResponse<string>>(this.baseUrl, request, { params });
  }

  updateTask(id: string, request: UpdateTaskRequest) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, request);
  }

  deleteTask(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }

  // Cursor/keyset pagination — más eficiente para listas largas
  getTasksByBoardWithCursor(boardId: string, pageSize = 20, cursor?: string) {
    let params = new HttpParams()
      .set('boardId', boardId)
      .set('pageSize', pageSize);
    if (cursor) params = params.set('cursor', cursor);
    return this.http.get<ApiResponse<CursorPaginatedList<TaskItem>>>(
      `${this.baseUrl}/cursor`, { params }
    );
  }

  getComments(taskId: string, pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Comment>>>(
      `${this.baseUrl}/${taskId}/comments`, { params }
    );
  }

  addComment(taskId: string, content: string) {
    return this.http.post<ApiResponse<string>>(
      `${this.baseUrl}/${taskId}/comments`, { content }
    );
  }

  deleteComment(taskId: string, commentId: string) {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/${taskId}/comments/${commentId}`
    );
  }

  addTag(taskId: string, tagId: string) {
    return this.http.post<ApiResponse<void>>(
      `${this.baseUrl}/${taskId}/tags/${tagId}`, {}
    );
  }

  removeTag(taskId: string, tagId: string) {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/${taskId}/tags/${tagId}`
    );
  }
}
```

> Importar `CursorPaginatedList` y `Comment` desde sus modelos respectivos.

---

## 7. TagsService

```typescript
// src/app/core/services/tags.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/board.models';
import { Tag } from '../models/tag.models';

@Injectable({ providedIn: 'root' })
export class TagsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tags`;

  getTags() {
    return this.http.get<ApiResponse<Tag[]>>(this.baseUrl);
  }

  createTag(name: string, color = '#6366F1') {
    return this.http.post<ApiResponse<string>>(this.baseUrl, { name, color });
  }

  deleteTag(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 8. UsersService

```typescript
// src/app/core/services/users.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/board.models';
import { User } from '../models/user.models';
import { UserRole } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Users`;

  getUsers(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<User>>>(this.baseUrl, { params });
  }

  getMe() {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/me`);
  }

  updateMe(firstName: string, lastName: string) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/me`, { firstName, lastName });
  }

  getUserById(id: string) {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/${id}`);
  }

  changeRole(id: string, role: UserRole) {
    return this.http.patch<ApiResponse<void>>(`${this.baseUrl}/${id}/role`, { role });
  }

  deleteUser(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 9. ClientsService

```typescript
// src/app/core/services/clients.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/board.models';
import { Client } from '../models/client.models';

export interface ClientRequest {
  name: string;
  email: string;
  phone?: string;
  company?: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Clients`;

  getClients(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Client>>>(this.baseUrl, { params });
  }

  getClientById(id: string) {
    return this.http.get<ApiResponse<Client>>(`${this.baseUrl}/${id}`);
  }

  createClient(request: ClientRequest) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, request);
  }

  updateClient(id: string, request: ClientRequest) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, request);
  }

  deleteClient(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 10. ProjectsService

```typescript
// src/app/core/services/projects.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/board.models';
import { Project, ProjectStatus } from '../models/project.models';

export interface ProjectRequest {
  title: string;
  description?: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
  clientId: string;
  analystId: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Projects`;

  getProjects(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Project>>>(this.baseUrl, { params });
  }

  getProjectById(id: string) {
    return this.http.get<ApiResponse<Project>>(`${this.baseUrl}/${id}`);
  }

  createProject(request: ProjectRequest) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, request);
  }

  updateProject(id: string, request: ProjectRequest) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, request);
  }

  deleteProject(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 11. Guard de autenticación

```typescript
// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  return true;
};
```

```typescript
// src/app/core/guards/admin.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAdmin()) return true;

  return router.createUrlTree(['/forbidden']);
};
```

---

## 12. Rutas

```typescript
// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component')
      .then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component')
      .then(m => m.RegisterComponent)
  },
  {
    path: 'boards',
    canActivate: [authGuard],
    loadComponent: () => import('./features/boards/boards-list/boards-list.component')
      .then(m => m.BoardsListComponent)
  },
  {
    path: 'boards/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/boards/board-detail/board-detail.component')
      .then(m => m.BoardDetailComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/admin.component')
      .then(m => m.AdminComponent)
  },
  { path: '', redirectTo: 'boards', pathMatch: 'full' },
  { path: '**', redirectTo: 'boards' }
];
```

---

## 13. Componente Login (ejemplo completo)

```typescript
// src/app/features/auth/login/login.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="login-container">
      <h2>Iniciar sesión</h2>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <input formControlName="email" type="email" placeholder="Email" />
        <input formControlName="password" type="password" placeholder="Contraseña" />

        @if (error()) {
          <p class="error">{{ error() }}</p>
        }

        <button type="submit" [disabled]="loading()">
          {{ loading() ? 'Cargando...' : 'Entrar' }}
        </button>
      </form>

      <a routerLink="/register">¿No tienes cuenta? Regístrate</a>
    </div>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal('');

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set('');

    this.auth.login(this.form.value as any).subscribe({
      next: () => this.router.navigate(['/boards']),
      error: err => {
        this.error.set(err.error?.message ?? 'Error al iniciar sesión');
        this.loading.set(false);
      }
    });
  }
}
```

---

## 14. Componente Register

```typescript
// src/app/features/auth/register/register.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

function passwordStrength(control: AbstractControl): ValidationErrors | null {
  const v: string = control.value ?? '';
  const ok = /[A-Z]/.test(v) && /[a-z]/.test(v) && /\d/.test(v) && /[^A-Za-z\d]/.test(v);
  return ok ? null : { passwordStrength: true };
}

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const pw = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pw === confirm ? null : { passwordsMismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h2>Crear cuenta</h2>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-row">
          <input formControlName="firstName" placeholder="Nombre *" />
          @if (form.get('firstName')?.touched && form.get('firstName')?.invalid) {
            <span class="field-error">Nombre requerido</span>
          }
        </div>

        <div class="form-row">
          <input formControlName="lastName" placeholder="Apellido *" />
          @if (form.get('lastName')?.touched && form.get('lastName')?.invalid) {
            <span class="field-error">Apellido requerido</span>
          }
        </div>

        <div class="form-row">
          <input formControlName="email" type="email" placeholder="Email *" />
          @if (form.get('email')?.touched && form.get('email')?.invalid) {
            <span class="field-error">Email inválido</span>
          }
        </div>

        <div class="form-row">
          <input formControlName="password" type="password" placeholder="Contraseña *" />
          @if (form.get('password')?.touched && form.get('password')?.hasError('passwordStrength')) {
            <span class="field-error">
              Debe tener mayúscula, minúscula, número y símbolo
            </span>
          }
        </div>

        <div class="form-row">
          <input formControlName="confirmPassword" type="password" placeholder="Confirmar contraseña *" />
          @if (form.hasError('passwordsMismatch') && form.get('confirmPassword')?.touched) {
            <span class="field-error">Las contraseñas no coinciden</span>
          }
        </div>

        @if (error()) {
          <p class="error">{{ error() }}</p>
        }

        <button type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Creando cuenta...' : 'Registrarse' }}
        </button>
      </form>

      <a routerLink="/login">¿Ya tienes cuenta? Inicia sesión</a>
    </div>
  `
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal('');

  form = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName:  ['', [Validators.required, Validators.maxLength(100)]],
    email:     ['', [Validators.required, Validators.email]],
    password:  ['', [Validators.required, Validators.minLength(8), passwordStrength]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordsMatch });

  onSubmit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set('');

    const { firstName, lastName, email, password } = this.form.value;

    this.auth.register({ firstName: firstName!, lastName: lastName!, email: email!, password: password! })
      .subscribe({
        next: () => this.router.navigate(['/boards']),
        error: err => {
          const apiErrors: string[] = err.error?.errors ?? [];
          this.error.set(apiErrors.length ? apiErrors.join(' · ') : (err.error?.message ?? 'Error al registrarse'));
          this.loading.set(false);
        }
      });
  }
}
```

---

## 15. BoardsList — Create + Update + Delete

```typescript
// src/app/features/boards/boards-list/boards-list.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BoardsService } from '../../../core/services/boards.service';
import { Board, PaginatedList } from '../../../core/models/board.models';

@Component({
  selector: 'app-boards-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  template: `
    <div class="boards-header">
      <h2>Mis Boards</h2>
      <button (click)="showCreate.set(true)">+ Nuevo Board</button>
    </div>

    <!-- Modal crear board -->
    @if (showCreate()) {
      <div class="modal-overlay" (click)="cancelCreate()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Nuevo Board</h3>
          <form [formGroup]="createForm" (ngSubmit)="createBoard()">
            <input formControlName="title" placeholder="Título *" />
            <textarea formControlName="description" placeholder="Descripción (opcional)"></textarea>
            @if (formError()) { <p class="error">{{ formError() }}</p> }
            <div class="modal-actions">
              <button type="button" (click)="cancelCreate()">Cancelar</button>
              <button type="submit" [disabled]="createForm.invalid || saving()">
                {{ saving() ? 'Guardando...' : 'Crear' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- Modal editar board -->
    @if (editingBoard()) {
      <div class="modal-overlay" (click)="cancelEdit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Editar Board</h3>
          <form [formGroup]="editForm" (ngSubmit)="updateBoard()">
            <input formControlName="title" placeholder="Título *" />
            <textarea formControlName="description" placeholder="Descripción (opcional)"></textarea>
            @if (formError()) { <p class="error">{{ formError() }}</p> }
            <div class="modal-actions">
              <button type="button" (click)="cancelEdit()">Cancelar</button>
              <button type="submit" [disabled]="editForm.invalid || saving()">
                {{ saving() ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    @if (loading()) {
      <p>Cargando...</p>
    } @else {
      <div class="boards-grid">
        @for (board of paginated()?.items; track board.id) {
          <div class="board-card">
            <div [routerLink]="['/boards', board.id]" class="board-link">
              <h3>{{ board.title }}</h3>
              <p>{{ board.description }}</p>
              <span>{{ board.taskCount }} tareas</span>
            </div>
            <div class="board-actions">
              <button (click)="openEdit(board)">Editar</button>
              <button class="danger" (click)="deleteBoard(board.id)">Eliminar</button>
            </div>
          </div>
        }
      </div>

      @if (paginated(); as p) {
        <div class="pagination">
          <button (click)="prevPage()" [disabled]="!p.hasPreviousPage">Anterior</button>
          <span>{{ p.pageNumber }} / {{ p.totalPages }}</span>
          <button (click)="nextPage()" [disabled]="!p.hasNextPage">Siguiente</button>
        </div>
      }
    }
  `
})
export class BoardsListComponent implements OnInit {
  private readonly boardsService = inject(BoardsService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly paginated = signal<PaginatedList<Board> | null>(null);
  readonly currentPage = signal(1);
  readonly showCreate = signal(false);
  readonly editingBoard = signal<Board | null>(null);
  readonly formError = signal('');

  createForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  editForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.boardsService.getBoards(this.currentPage(), 20).subscribe({
      next: res => { this.paginated.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  createBoard() {
    if (this.createForm.invalid) return;
    this.saving.set(true);
    this.formError.set('');
    const { title, description } = this.createForm.value;
    this.boardsService.createBoard(title!, description ?? undefined).subscribe({
      next: () => { this.cancelCreate(); this.load(); },
      error: err => {
        this.formError.set(err.error?.message ?? 'Error al crear');
        this.saving.set(false);
      }
    });
  }

  openEdit(board: Board) {
    this.editingBoard.set(board);
    this.editForm.setValue({ title: board.title, description: board.description ?? '' });
    this.formError.set('');
  }

  updateBoard() {
    const board = this.editingBoard();
    if (!board || this.editForm.invalid) return;
    this.saving.set(true);
    this.formError.set('');
    const { title, description } = this.editForm.value;
    this.boardsService.updateBoard(board.id, title!, description ?? undefined).subscribe({
      next: () => { this.cancelEdit(); this.load(); },
      error: err => {
        this.formError.set(err.error?.message ?? 'Error al actualizar');
        this.saving.set(false);
      }
    });
  }

  deleteBoard(id: string) {
    if (!confirm('¿Eliminar este board? También se eliminarán sus tareas.')) return;
    this.boardsService.deleteBoard(id).subscribe({ next: () => this.load() });
  }

  cancelCreate() {
    this.showCreate.set(false);
    this.createForm.reset();
    this.saving.set(false);
    this.formError.set('');
  }

  cancelEdit() {
    this.editingBoard.set(null);
    this.saving.set(false);
    this.formError.set('');
  }

  nextPage() { this.currentPage.update(p => p + 1); this.load(); }
  prevPage() { this.currentPage.update(p => p - 1); this.load(); }
}
```

---

## 16. BoardDetail — Tasks CRUD completo

```typescript
// src/app/features/boards/board-detail/board-detail.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BoardsService } from '../../../core/services/boards.service';
import { TasksService, CreateTaskRequest, UpdateTaskRequest } from '../../../core/services/tasks.service';
import { Board } from '../../../core/models/board.models';
import { TaskItem, TaskItemStatus, TaskPriority } from '../../../core/models/task.models';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="board-header">
      <h2>{{ board()?.title }}</h2>
      <p>{{ board()?.description }}</p>
      <button (click)="showCreate.set(true)">+ Nueva Tarea</button>
    </div>

    <!-- Modal crear tarea -->
    @if (showCreate()) {
      <div class="modal-overlay" (click)="cancelCreate()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Nueva Tarea</h3>
          <form [formGroup]="createForm" (ngSubmit)="createTask()">
            <input formControlName="title" placeholder="Título *" />
            <textarea formControlName="description" placeholder="Descripción"></textarea>
            <select formControlName="priority">
              @for (p of priorities; track p) {
                <option [value]="p">{{ p }}</option>
              }
            </select>
            <input formControlName="dueDate" type="date" />
            @if (formError()) { <p class="error">{{ formError() }}</p> }
            <div class="modal-actions">
              <button type="button" (click)="cancelCreate()">Cancelar</button>
              <button type="submit" [disabled]="createForm.invalid || saving()">
                {{ saving() ? 'Guardando...' : 'Crear' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- Modal editar tarea -->
    @if (editingTask()) {
      <div class="modal-overlay" (click)="cancelEdit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Editar Tarea</h3>
          <form [formGroup]="editForm" (ngSubmit)="updateTask()">
            <input formControlName="title" placeholder="Título *" />
            <textarea formControlName="description" placeholder="Descripción"></textarea>
            <select formControlName="status">
              @for (s of statuses; track s) {
                <option [value]="s">{{ s }}</option>
              }
            </select>
            <select formControlName="priority">
              @for (p of priorities; track p) {
                <option [value]="p">{{ p }}</option>
              }
            </select>
            <input formControlName="dueDate" type="date" />
            @if (formError()) { <p class="error">{{ formError() }}</p> }
            <div class="modal-actions">
              <button type="button" (click)="cancelEdit()">Cancelar</button>
              <button type="submit" [disabled]="editForm.invalid || saving()">
                {{ saving() ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    @if (loading()) {
      <p>Cargando tareas...</p>
    } @else {
      <div class="tasks-list">
        @for (task of tasks(); track task.id) {
          <div class="task-card">
            <div class="task-info">
              <h4>{{ task.title }}</h4>
              <p>{{ task.description }}</p>
              <div class="task-meta">
                <span class="badge status">{{ task.status }}</span>
                <span class="badge priority">{{ task.priority }}</span>
                @if (task.dueDate) {
                  <span class="due">{{ task.dueDate | date:'dd/MM/yyyy' }}</span>
                }
              </div>
            </div>
            <div class="task-actions">
              <button (click)="openEdit(task)">Editar</button>
              <button class="danger" (click)="deleteTask(task.id)">Eliminar</button>
            </div>
          </div>
        }

        @if (tasks().length === 0) {
          <p class="empty">No hay tareas. ¡Crea la primera!</p>
        }
      </div>

      @if (totalPages() > 1) {
        <div class="pagination">
          <button (click)="prevPage()" [disabled]="currentPage() === 1">Anterior</button>
          <span>{{ currentPage() }} / {{ totalPages() }}</span>
          <button (click)="nextPage()" [disabled]="currentPage() === totalPages()">Siguiente</button>
        </div>
      }
    }
  `
})
export class BoardDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly boardsService = inject(BoardsService);
  private readonly tasksService = inject(TasksService);
  private readonly fb = inject(FormBuilder);

  readonly board = signal<Board | null>(null);
  readonly tasks = signal<TaskItem[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showCreate = signal(false);
  readonly editingTask = signal<TaskItem | null>(null);
  readonly formError = signal('');
  readonly currentPage = signal(1);
  readonly totalPages = signal(1);

  readonly statuses = Object.values(TaskItemStatus);
  readonly priorities = Object.values(TaskPriority);

  private boardId!: string;

  createForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    priority: [TaskPriority.Medium, Validators.required],
    dueDate: ['']
  });

  editForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    status: [TaskItemStatus.Todo, Validators.required],
    priority: [TaskPriority.Medium, Validators.required],
    dueDate: ['']
  });

  ngOnInit() {
    this.boardId = this.route.snapshot.paramMap.get('id')!;
    this.boardsService.getBoardById(this.boardId).subscribe({
      next: res => this.board.set(res.data)
    });
    this.loadTasks();
  }

  loadTasks() {
    this.loading.set(true);
    this.tasksService.getTasksByBoard(this.boardId, this.currentPage(), 50).subscribe({
      next: res => {
        this.tasks.set(res.data.items);
        this.totalPages.set(res.data.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  createTask() {
    if (this.createForm.invalid) return;
    this.saving.set(true);
    this.formError.set('');
    const v = this.createForm.value;
    const request: CreateTaskRequest = {
      title: v.title!,
      description: v.description || undefined,
      priority: v.priority as TaskPriority,
      dueDate: v.dueDate || undefined
    };
    this.tasksService.createTask(this.boardId, request).subscribe({
      next: () => { this.cancelCreate(); this.loadTasks(); },
      error: err => {
        this.formError.set(err.error?.message ?? 'Error al crear tarea');
        this.saving.set(false);
      }
    });
  }

  openEdit(task: TaskItem) {
    this.editingTask.set(task);
    this.editForm.setValue({
      title: task.title,
      description: task.description ?? '',
      status: task.status,
      priority: task.priority,
      dueDate: task.dueDate ? task.dueDate.substring(0, 10) : ''
    });
    this.formError.set('');
  }

  updateTask() {
    const task = this.editingTask();
    if (!task || this.editForm.invalid) return;
    this.saving.set(true);
    this.formError.set('');
    const v = this.editForm.value;
    const request: UpdateTaskRequest = {
      title: v.title!,
      description: v.description || undefined,
      status: v.status as TaskItemStatus,
      priority: v.priority as TaskPriority,
      dueDate: v.dueDate || undefined
    };
    this.tasksService.updateTask(task.id, request).subscribe({
      next: () => { this.cancelEdit(); this.loadTasks(); },
      error: err => {
        this.formError.set(err.error?.message ?? 'Error al actualizar');
        this.saving.set(false);
      }
    });
  }

  deleteTask(id: string) {
    if (!confirm('¿Eliminar esta tarea?')) return;
    this.tasksService.deleteTask(id).subscribe({ next: () => this.loadTasks() });
  }

  cancelCreate() {
    this.showCreate.set(false);
    this.createForm.reset({ priority: TaskPriority.Medium });
    this.saving.set(false);
    this.formError.set('');
  }

  cancelEdit() {
    this.editingTask.set(null);
    this.saving.set(false);
    this.formError.set('');
  }

  nextPage() { this.currentPage.update(p => p + 1); this.loadTasks(); }
  prevPage() { this.currentPage.update(p => p - 1); this.loadTasks(); }
}
```

---

## 17. Manejo de CORS

La API ya tiene CORS configurado para `http://localhost:3000`. Para Angular (puerto 4200 por defecto), actualizar `appsettings.json`:

```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:4200"
]
```

O en `compose.yaml`:

```yaml
- AllowedOrigins__0=http://localhost:4200
```

---

## 18. Flujo completo de tokens

```
1. Login / Register → recibe accessToken (15 min) + refreshToken (7 días)
2. Cada request → interceptor agrega Authorization: Bearer <accessToken>
3. Si la API devuelve 401 → interceptor llama a /Auth/refresh automáticamente
4. Si el refresh falla → redirige a /login
5. Logout → la API invalida el refreshToken en DB
```

---

## 19. Estructura recomendada del proyecto Angular

```
src/app/
├── core/
│   ├── guards/
│   │   ├── auth.guard.ts
│   │   └── admin.guard.ts
│   ├── interceptors/
│   │   └── auth.interceptor.ts
│   ├── models/
│   │   ├── auth.models.ts       UserRole enum, AuthResponse, LoginRequest, RegisterRequest
│   │   ├── board.models.ts      Board, PaginatedList<T>, CursorPaginatedList<T>, ApiResponse<T>
│   │   ├── task.models.ts       TaskItem, TaskItemStatus, TaskPriority
│   │   ├── comment.models.ts    Comment
│   │   ├── tag.models.ts        Tag
│   │   ├── client.models.ts     Client
│   │   ├── project.models.ts    Project, ProjectStatus
│   │   └── user.models.ts       User
│   └── services/
│       ├── auth.service.ts
│       ├── boards.service.ts
│       ├── tasks.service.ts     (incluye comments + tags sub-recursos)
│       ├── tags.service.ts
│       ├── users.service.ts
│       ├── clients.service.ts
│       └── projects.service.ts
├── features/
│   ├── auth/
│   │   ├── login/               login.component.ts
│   │   └── register/            register.component.ts
│   ├── boards/
│   │   ├── boards-list/         boards-list.component.ts
│   │   └── board-detail/        board-detail.component.ts
│   ├── admin/
│   │   └── admin.component.ts
│   ├── clients/
│   │   └── clients-list.component.ts
│   └── projects/
│       └── projects-list.component.ts
├── shared/
│   └── components/              button, input, modal, tag-badge, ...
├── app.config.ts
├── app.routes.ts
└── app.component.ts
```

---

## Tips

- Usar `toSignal()` de `@angular/core/rxjs-interop` para convertir observables a signals sin subscribe manual.
- El campo `role` en `AuthResponse` es numérico (`0`=Member, `1`=Admin, `2`=Analyst, `3`=Client) — comparar con el enum `UserRole`.
- La API devuelve `errors: string[] | null` en las respuestas fallidas — mostrarlo en los formularios.
- `GET /api/Tags` está cacheado 10 min en el servidor — no hace falta cachear en el cliente; un simple `signal<Tag[]>([])` es suficiente.
- Para cursor pagination (`GET /api/Tasks/cursor`): guardar el `nextCursor` devuelto y pasarlo como `cursor=...` en la siguiente petición. Cuando `hasNextPage` es `false` no hay más páginas.
- La paginación offset (`PaginatedList<T>`) sirve para Boards y vistas paginadas clásicas. La paginación cursor (`CursorPaginatedList<T>`) es mejor para listas largas de tareas (más eficiente en DB).
- Para roles: usar `auth.isAdmin()` para botones Admin, `auth.isAdminOrAnalyst()` para acciones que permiten Analyst también (crear clientes/proyectos).
- Para ambientes con HTTPS en producción, cambiar `apiUrl` en el environment de producción.
