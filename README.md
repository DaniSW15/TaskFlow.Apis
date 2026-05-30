# TaskFlow API

REST API de gestión de tareas construida con **.NET 10**, Clean Architecture, CQRS y JWT.

Proyecto de práctica para aprender arquitectura de backend profesional: modelado de base de datos relacional, patrones de diseño, autenticación, paginación eficiente y testing.

---

## Stack

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10.0 | Runtime |
| ASP.NET Core | 10.0 | Framework HTTP |
| Entity Framework Core | 9.0.4 | ORM |
| PostgreSQL | 17 | Base de datos |
| MediatR | 12.4.1 | CQRS / Mediator |
| FluentValidation | 11.11.0 | Validación |
| BCrypt.Net-Next | 4.0.3 | Hash de contraseñas |
| JWT Bearer | 9.0.5 | Autenticación |
| Swashbuckle | 7.3.1 | Swagger UI |
| Docker | — | Contenedores |
| xUnit + Moq | — | Unit tests |
| FluentAssertions | 8.x | Assertions en tests |

---

## Arquitectura

```
TaskFlow.Shared         → tipos comunes (Result, Error, PaginatedList)
TaskFlow.Domain         → entidades, enums, interfaces de repositorio
TaskFlow.Application    → CQRS (commands/queries/handlers), DTOs, validaciones
TaskFlow.Infrastructure → EF Core, repositorios, JWT, BCrypt
TaskFlow.Apis           → controllers, middleware, Program.cs
```

Las dependencias fluyen hacia adentro: `Apis → Application → Domain ← Infrastructure`.

### Pipeline de MediatR

```
Request → LoggingBehavior → CachingBehavior → ValidationBehavior → Handler
```

`CachingBehavior` cortocircuita el pipeline en cache hit — ni la validación ni el handler se ejecutan.

---

## Estructura de carpetas

```
TaskFlow.Apis/
├── Controllers/       AuthController, BoardsController, TasksController
├── Middlware/         ExceptionHandlingMiddleware, SecurityHeadersMiddleware
├── Extensions/        SwaggerExtensions
├── Services/          CurrentUserService
├── Program.cs
└── compose.yaml

TaskFlow.Application/
├── Features/
│   ├── Auth/Commands/ Login, Register, Logout, RefreshToken
│   ├── Boards/        Commands (Create, Update, Delete) + Queries (GetBoards, GetById)
│   └── Tasks/         Commands (Create, Update, Delete) + Queries (GetByBoard, GetById)
├── Behaviors/         LoggingBehavior, ValidationBehavior
├── DTOs/              AuthResponse, BoardDto, TaskDto, ...
└── Interfaces/        ICurrentUserService, IPasswordService, ITokenService

TaskFlow.Domain/
├── Entities/          User, Board, TaskItem, Client, Project, Comment, Tag
├── Enums/             TaskItemStatus, TaskPriority, UserRole, ProjectStatus
├── Common/            BaseEntity
└── Interfaces/        IUnitOfWork, IUserRepository, IBoardRepository, ITaskRepository,
                       IClientRepository, IProjectRepository, ICommentRepository, ITagRepository

TaskFlow.Infrastructure/
├── Persistence/       AppDbContext + EF configurations
├── Repositories/      UserRepository, BoardRepository, TaskRepository, UnitOfWork,
                       ClientRepository, ProjectRepository, CommentRepository, TagRepository
├── Services/          TokenService, PasswordService, CacheService
├── Configurations/    JwtSettings
└── Migrations/

TaskFlow.Shared/
└── Common/            Result<T>, Error, ApiResponse<T>, PaginatedList<T>, CursorPaginatedList<T>

TaskFlow.Tests/
└── Features/          20 unit tests (Tags, Comments, relación N:M)
```

---

## Ejecutar con Docker

```bash
# Desde TaskFlow.Apis/
docker compose up --build
```

La API queda disponible en `http://localhost:8080`.
Swagger UI: `http://localhost:8080/swagger`.

Las migraciones se aplican automáticamente al arrancar el contenedor.

### Variables de entorno

| Variable | Default | Descripción |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=postgres;...` | Cadena de conexión PostgreSQL |
| `JwtSettings__SecretKey` | `change-me-in-production-32chars!!` | **Cambiar en producción** |
| `JwtSettings__Issuer` | `TaskFlow.Api` | Issuer del JWT |
| `JwtSettings__Audience` | `TaskFlow.Client` | Audience del JWT |

---

## Ejecutar localmente (sin Docker)

```bash
# Levantar PostgreSQL
docker run -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=taskflow -p 5432:5432 postgres:17-alpine

# Aplicar migraciones
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Apis

# Ejecutar
dotnet run --project TaskFlow.Apis
```

---

## Endpoints

### Auth

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/api/Auth/register` | ❌ | Crear cuenta |
| `POST` | `/api/Auth/login` | ❌ | Iniciar sesión |

### Users

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/Users` | Admin | Listar todos los usuarios |
| `GET` | `/api/Users/me` | Cualquiera | Perfil propio |
| `PUT` | `/api/Users/me` | Cualquiera | Actualizar perfil propio |
| `GET` | `/api/Users/{id}` | Admin | Obtener usuario por ID |
| `PATCH` | `/api/Users/{id}/role` | Admin | Cambiar rol |
| `DELETE` | `/api/Users/{id}` | Admin | Borrar usuario |

### Boards

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/Boards?pageNumber=1&pageSize=20` | Listar boards del usuario |
| `GET` | `/api/Boards/{id}` | Obtener board por ID |
| `POST` | `/api/Boards` | Crear board |
| `PUT` | `/api/Boards/{id}` | Actualizar board |
| `DELETE` | `/api/Boards/{id}` | Borrar board |

### Tasks

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/Tasks?boardId={id}&pageNumber=1&pageSize=50` | Paginación offset |
| `GET` | `/api/Tasks/cursor?boardId={id}&cursor=&pageSize=50` | Paginación por cursor (keyset) |
| `GET` | `/api/Tasks/{id}` | Obtener tarea por ID |
| `POST` | `/api/Tasks?boardId={id}` | Crear tarea |
| `PUT` | `/api/Tasks/{id}` | Actualizar tarea |
| `DELETE` | `/api/Tasks/{id}` | Borrar tarea |

### Comments (sub-recurso de Task)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/Tasks/{taskId}/comments` | Listar comentarios paginados |
| `POST` | `/api/Tasks/{taskId}/comments` | Agregar comentario |
| `DELETE` | `/api/Tasks/{taskId}/comments/{id}` | Borrar (solo autor o Admin) |

### Tags

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/Tags` | Cualquiera | Listar todos los tags |
| `POST` | `/api/Tags` | Admin | Crear tag |
| `DELETE` | `/api/Tags/{id}` | Admin | Borrar tag |
| `POST` | `/api/Tasks/{taskId}/tags/{tagId}` | Cualquiera | Asociar tag a tarea (N:M) |
| `DELETE` | `/api/Tasks/{taskId}/tags/{tagId}` | Cualquiera | Desasociar tag de tarea |

### Clients

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/Clients` | Cualquiera | Listar clientes paginados |
| `GET` | `/api/Clients/{id}` | Cualquiera | Obtener cliente |
| `POST` | `/api/Clients` | Admin, Analyst | Crear cliente |
| `PUT` | `/api/Clients/{id}` | Admin, Analyst | Actualizar cliente |
| `DELETE` | `/api/Clients/{id}` | Admin | Borrar cliente |

### Projects

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/Projects` | Cualquiera | Listar proyectos paginados |
| `GET` | `/api/Projects/{id}` | Cualquiera | Obtener proyecto con boards |
| `POST` | `/api/Projects` | Admin, Analyst | Crear proyecto |
| `PUT` | `/api/Projects/{id}` | Admin, Analyst | Actualizar proyecto |
| `DELETE` | `/api/Projects/{id}` | Admin | Borrar proyecto |

Todos los endpoints requieren `Authorization: Bearer <token>` salvo los de Auth.

---

## Autenticación

```
Authorization: Bearer <accessToken>
```

El `accessToken` expira en **15 minutos**. Usar el `refreshToken` (7 días) para renovarlo sin re-autenticar.

---

## Features implementados

### Esquema de BD (7 tablas + 1 join table)

```
Users ──< Projects (AnalystId FK)
Clients ──< Projects (ClientId FK)
Projects ──< Boards (ProjectId FK, nullable)
Boards ──< TaskItems (BoardId FK)
TaskItems ──< Comments ──> Users (AuthorId FK)
TaskItems >──< Tags    (join table: TaskItemTags)
```

### Roles

| Rol | Permisos |
|---|---|
| `Member` | Leer/escribir sus propios boards y tareas, agregar comentarios |
| `Analyst` | Todo lo anterior + gestionar clientes y proyectos |
| `Admin` | Acceso total: usuarios, tags, roles |
| `Client` | Solo lectura en proyectos asignados |

### Soft-delete
Delete no borra físicamente — marca `IsDeleted = true` y guarda `DeletedAt` / `DeletedBy`. Los global query filters de EF Core excluyen registros borrados de todas las queries automáticamente.

### Paginación

**Offset** (clásico):
```json
{ "items": [...], "totalCount": 42, "pageNumber": 1, "pageSize": 20, "totalPages": 3, "hasNextPage": true }
```

**Cursor / keyset** (eficiente en tablas grandes):
```json
{ "items": [...], "nextCursor": "eyJDcmVhdGVkQXQi...", "hasNextPage": true, "pageSize": 20 }
```
Pasar `nextCursor` como `?cursor=` en el siguiente request. No ejecuta `COUNT(*)` ni `OFFSET`.

### Caché

`GET /api/Tags` cachea el resultado por 10 minutos con `IMemoryCache`. El caché se invalida automáticamente al crear o borrar un tag. Implementado como **pipeline behavior de MediatR** — los handlers no tienen ningún conocimiento del caché.

### Unit Tests

```bash
dotnet test TaskFlow.Tests/TaskFlow.Tests.csproj
# Correctas! - 20/20
```

Tests cubren: `CreateTag`, `DeleteTag`, `AddComment`, `DeleteComment`, `AddTagToTask` (relación N:M). Cada handler tiene test de éxito, test de error, y test que verifica que no se llama `SaveChangesAsync` en caso de fallo.

---

## Conexión a PostgreSQL

```
Host:     localhost  |  Port: 5432
Database: taskflow   |  User: postgres  |  Password: postgres
```

---

## Migraciones

```bash
# Crear
dotnet ef migrations add <Nombre> --project TaskFlow.Infrastructure --startup-project TaskFlow.Apis

# Aplicar
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Apis

# Revertir última
dotnet ef migrations remove --project TaskFlow.Infrastructure --startup-project TaskFlow.Apis
```

---

## CI/CD

| Workflow | Trigger | Acción |
|---|---|---|
| `ci.yml` | Push / PR a `main`, `develop` | `dotnet build` + `dotnet test` |
| `cd.yml` | Push a `main` o tag `v*` | Build Docker → push a GHCR |

Imagen publicada en `ghcr.io/<owner>/taskflow.apis`.
