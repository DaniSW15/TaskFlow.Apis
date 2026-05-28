# TaskFlow API

REST API de gestión de tareas construida con **.NET 10**, Clean Architecture, CQRS y JWT.

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
├── Entities/          User, Board, TaskItem
├── Enums/             TaskItemStatus, TaskPriority, UserRole
├── Common/            BaseEntity
└── Interfaces/        IUnitOfWork, IUserRepository, IBoardRepository, ITaskRepository

TaskFlow.Infrastructure/
├── Persistence/       AppDbContext + EF configurations
├── Repositories/      UserRepository, BoardRepository, TaskRepository, UnitOfWork
├── Services/          TokenService, PasswordService
├── Configurations/    JwtSettings
└── Migrations/

TaskFlow.Shared/
└── Common/            Result<T>, Error, ApiResponse<T>, PaginatedList<T>
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
| `POST` | `/api/Auth/refresh` | ❌ | Renovar access token |
| `POST` | `/api/Auth/logout` | ✅ | Cerrar sesión |

### Boards

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/Boards?pageNumber=1&pageSize=20` | Listar boards del usuario |
| `GET` | `/api/Boards/{id}` | Obtener board por ID |
| `POST` | `/api/Boards` | Crear board |
| `PUT` | `/api/Boards/{id}` | Actualizar board |
| `DELETE` | `/api/Boards/{id}` | Borrar board (soft-delete) |

### Tasks

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/Tasks?boardId={id}&pageNumber=1&pageSize=50` | Listar tareas de un board |
| `GET` | `/api/Tasks/{id}` | Obtener tarea por ID |
| `POST` | `/api/Tasks?boardId={id}` | Crear tarea |
| `PUT` | `/api/Tasks/{id}` | Actualizar tarea |
| `DELETE` | `/api/Tasks/{id}` | Borrar tarea (soft-delete) |

Todos los endpoints de Boards y Tasks requieren `Authorization: Bearer <accessToken>`.

---

## Autenticación

```
Authorization: Bearer <accessToken>
```

El `accessToken` expira en **15 minutos**. Usar el `refreshToken` (7 días) para renovarlo sin re-autenticar.

---

## Features implementados

### Roles
Los usuarios tienen rol `Member` (0) o `Admin` (1). El rol está en el JWT y se devuelve en cada respuesta de auth.

### Soft-delete
Delete no borra físicamente — marca `IsDeleted = true` y guarda `DeletedAt` / `DeletedBy`. Los global query filters de EF Core excluyen registros borrados de todas las queries automáticamente.

### Auditoría
Todas las entidades heredan de `BaseEntity`:
- `CreatedAt` / `UpdatedAt` — se setean automáticamente en `SaveChangesAsync`
- `CreatedBy` — ID del usuario que creó el registro
- `DeletedAt` / `DeletedBy` — se rellenan al hacer soft-delete

### Paginación
Los endpoints de colecciones devuelven `PaginatedList<T>`:

```json
{
  "items": [...],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Seguridad (OWASP)
- Headers: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Content-Security-Policy`, `Permissions-Policy`
- Rate limiting: 10 req/min en `/Auth/*`, 100 req/min general
- Contraseñas: BCrypt work factor 12
- JWT: `HmacSha256`, `ClockSkew = 0`, validación de issuer y audience

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
