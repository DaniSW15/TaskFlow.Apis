# TaskFlow API — Referencia

**Base URL local:** `http://localhost:8080/api`  
**Base URL producción:** `https://tu-dominio.com/api`  
**Swagger UI:** `http://localhost:8080/swagger`

---

## Formato de respuesta

Todos los endpoints devuelven el mismo wrapper:

```json
{
  "success": true,
  "data": { ... },
  "message": "OK",
  "errors": null
}
```

En caso de error:

```json
{
  "success": false,
  "data": null,
  "message": "Descripción del error",
  "errors": ["Detalle 1", "Detalle 2"]
}
```

---

## Autenticación

Los endpoints marcados con 🔒 requieren este header:

```
Authorization: Bearer <accessToken>
```

El `accessToken` dura **15 minutos**. Usar `/Auth/refresh` para obtener uno nuevo sin pedir contraseña.

---

## Códigos HTTP

| Código | Significado |
|---|---|
| `200` | OK |
| `201` | Creado |
| `400` | Datos inválidos — ver `errors[]` |
| `401` | Token inválido o expirado |
| `403` | No tienes permiso |
| `404` | No encontrado |
| `409` | Conflicto (ej. email ya existe) |
| `429` | Demasiadas peticiones |
| `500` | Error del servidor |

---

## Auth

### Registrar usuario

```
POST /api/Auth/register
```

**Body:**
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan@example.com",
  "password": "Segura1234!"
}
```

> La contraseña debe tener: mínimo 8 caracteres, mayúscula, minúscula, número y símbolo.

**Respuesta 201:**
```json
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

### Iniciar sesión

```
POST /api/Auth/login
```

**Body:**
```json
{
  "email": "juan@example.com",
  "password": "Segura1234!"
}
```

**Respuesta 200:** misma estructura que register.

---

### Renovar token

```
POST /api/Auth/refresh
```

**Body:**
```json
{
  "refreshToken": "abc123..."
}
```

**Respuesta 200:** misma estructura que login.

> Usar cuando el accessToken expire. El refreshToken dura **7 días**.

---

### Cerrar sesión 🔒

```
POST /api/Auth/logout
```

**Body:** vacío `{}`  
**Respuesta 200:** `{ "success": true, "data": null, ... }`

> Invalida el refreshToken en la base de datos.

---

## Boards 🔒

> Todos los endpoints de Boards requieren `Authorization: Bearer <token>`.  
> Solo ves tus propios boards. No puedes editar/eliminar boards de otros usuarios.

---

### Listar boards

```
GET /api/Boards?pageNumber=1&pageSize=20
```

| Parámetro | Tipo | Descripción |
|---|---|---|
| `pageNumber` | número | Página (default: 1) |
| `pageSize` | número | Resultados por página (default: 20) |

**Respuesta 200:**
```json
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

### Obtener board por ID

```
GET /api/Boards/{id}
```

**Respuesta 200:** objeto Board (igual que `items[]` arriba).  
**Respuesta 404:** si no existe o no es tuyo.

---

### Crear board

```
POST /api/Boards
```

**Body:**
```json
{
  "title": "Nuevo Board",
  "description": "Opcional"
}
```

**Respuesta 201:**
```json
{
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
> `data` es el ID del board creado (Guid).

---

### Actualizar board

```
PUT /api/Boards/{id}
```

**Body:**
```json
{
  "title": "Título actualizado",
  "description": "Nueva descripción"
}
```

**Respuesta 200:** `data: null`  
**Respuesta 403:** si no eres el dueño.  
**Respuesta 404:** si no existe.

---

### Eliminar board

```
DELETE /api/Boards/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, "message": "Board deleted successfully." }`  
**Respuesta 403:** si no eres el dueño.

> Soft-delete: no se borra físicamente. Las tareas del board también quedan eliminadas.

---

## Tasks 🔒

> Todos los endpoints de Tasks requieren `Authorization: Bearer <token>`.

**Valores válidos para `status`:**  
`"Todo"` | `"InProgress"` | `"Done"` | `"Cancelled"`

**Valores válidos para `priority`:**  
`"Low"` | `"Medium"` | `"High"` | `"Critical"`

---

### Listar tareas de un board

```
GET /api/Tasks?boardId={boardId}&pageNumber=1&pageSize=50
```

| Parámetro | Tipo | Descripción |
|---|---|---|
| `boardId` | Guid | **Requerido.** ID del board |
| `pageNumber` | número | Página (default: 1) |
| `pageSize` | número | Resultados por página (default: 50) |

**Respuesta 200:**
```json
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

---

### Obtener tarea por ID

```
GET /api/Tasks/{id}
```

**Respuesta 200:** objeto Task (igual que `items[]` arriba).

---

### Crear tarea

```
POST /api/Tasks?boardId={boardId}
```

> El `boardId` va en la **query string**, no en el body.

**Body:**
```json
{
  "title": "Nueva tarea",
  "description": "Opcional",
  "priority": "Medium",
  "dueDate": "2026-06-01T00:00:00Z",
  "assigneeId": null
}
```

| Campo | Tipo | Requerido |
|---|---|---|
| `title` | string | ✅ |
| `description` | string | ❌ |
| `priority` | string | ✅ |
| `dueDate` | ISO 8601 | ❌ |
| `assigneeId` | Guid | ❌ |

**Respuesta 201:**
```json
{
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
> `data` es el ID de la tarea creada.

---

### Actualizar tarea

```
PUT /api/Tasks/{id}
```

**Body:**
```json
{
  "title": "Título actualizado",
  "description": "Opcional",
  "status": "InProgress",
  "priority": "High",
  "dueDate": "2026-06-15T00:00:00Z",
  "assigneeId": null
}
```

| Campo | Tipo | Requerido |
|---|---|---|
| `title` | string | ✅ |
| `description` | string | ❌ |
| `status` | string | ✅ |
| `priority` | string | ✅ |
| `dueDate` | ISO 8601 | ❌ |
| `assigneeId` | Guid | ❌ |

**Respuesta 200:** `data: null`  
**Respuesta 403:** si no tienes permiso.

---

### Eliminar tarea

```
DELETE /api/Tasks/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, "message": "Task deleted successfully." }`

> Soft-delete: no se borra físicamente.

---

### Listar tareas con cursor (keyset pagination)

```
GET /api/Tasks/cursor?boardId={boardId}&pageSize=50&cursor={cursor}
```

| Parámetro | Tipo | Descripción |
|---|---|---|
| `boardId` | Guid | **Requerido.** ID del board |
| `pageSize` | número | Resultados por página (default: 50) |
| `cursor` | string | Omitir en primera página. Usar `nextCursor` de la respuesta anterior |

**Respuesta 200:**
```json
{
  "items": [ /* igual que la paginación offset */ ],
  "nextCursor": "MjAyNi0wNS0yN1QxMDowMDowMHwzZmE4NWY2NC0...",
  "hasNextPage": true,
  "pageSize": 50
}
```

> Cuando `hasNextPage` es `false`, no hay más páginas.  
> El cursor es opaco (Base64 de `createdAt|id`) — no lo construyas manualmente.  
> Más eficiente que offset: evita `OFFSET` lento y funciona bien con inserciones en tiempo real.

---

## Comments 🔒

> Sub-recurso de Task. Todos requieren `Authorization: Bearer <token>`.

### Listar comentarios de una tarea

```
GET /api/Tasks/{taskId}/comments?pageNumber=1&pageSize=20
```

**Respuesta 200:**
```json
{
  "items": [
    {
      "id": "3fa85f64-...",
      "content": "Revisar la validación del token",
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

---

### Añadir comentario

```
POST /api/Tasks/{taskId}/comments
```

**Body:**
```json
{ "content": "Mi comentario sobre esta tarea" }
```

| Campo | Restricción |
|---|---|
| `content` | 1–2000 caracteres, requerido |

**Respuesta 201:** `{ "data": "3fa85f64-..." }` (Guid del comentario)

---

### Eliminar comentario

```
DELETE /api/Tasks/{taskId}/comments/{commentId}
```

**Respuesta 200:** `{ "success": true, "data": null, ... }`  
**Respuesta 403:** si no eres el autor del comentario.

---

## Tags 🔒

### Listar todos los tags

```
GET /api/Tags
```

**Respuesta 200:**
```json
[
  { "id": "3fa85f64-...", "name": "Bug", "color": "#EF4444" },
  { "id": "3fa85f64-...", "name": "Feature", "color": "#6366F1" }
]
```

> Respuesta **cacheada 10 minutos** en servidor. Se invalida al crear/eliminar un tag.

---

### Crear tag — solo Admin

```
POST /api/Tags
```

**Body:**
```json
{ "name": "Urgente", "color": "#F59E0B" }
```

| Campo | Tipo | Requerido | Restricciones |
|---|---|---|---|
| `name` | string | ✅ | 1–50 chars, único |
| `color` | string | ❌ | Hex válido (default: `#6366F1`) |

**Respuesta 201:** `{ "data": "3fa85f64-..." }`

---

### Eliminar tag — solo Admin

```
DELETE /api/Tags/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, ... }`  
**Respuesta 404:** si no existe.

---

### Asociar tag a tarea

```
POST /api/Tasks/{taskId}/tags/{tagId}
```

**Body:** vacío.  
**Respuesta 200:** `{ "success": true, ... }`  

> Idempotente: si la tarea ya tiene ese tag, responde 200 sin duplicar.

---

### Desasociar tag de tarea

```
DELETE /api/Tasks/{taskId}/tags/{tagId}
```

**Respuesta 200:** `{ "success": true, ... }`  

> Idempotente: si la tarea no tiene ese tag, responde 200 igualmente.

---

## Users 🔒

### Ver mi perfil

```
GET /api/Users/me
```

**Respuesta 200:**
```json
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
```

---

### Actualizar mi perfil

```
PUT /api/Users/me
```

**Body:** `{ "firstName": "Juan", "lastName": "García" }`  
**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

### Listar todos los usuarios — solo Admin

```
GET /api/Users?pageNumber=1&pageSize=20
```

**Respuesta 200:** `PaginatedList<UserDto>` (misma estructura que Boards).

---

### Ver usuario por ID — solo Admin

```
GET /api/Users/{id}
```

**Respuesta 200:** objeto UserDto.  
**Respuesta 404:** si no existe.

---

### Cambiar rol de usuario — solo Admin

```
PATCH /api/Users/{id}/role
```

**Body:** `{ "role": 2 }`  
> `role`: `0`=Member, `1`=Admin, `2`=Analyst, `3`=Client

**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

### Eliminar usuario — solo Admin

```
DELETE /api/Users/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, ... }`  
**Respuesta 404:** si no existe.

---

## Clients 🔒

### Listar clientes

```
GET /api/Clients?pageNumber=1&pageSize=20
```

**Respuesta 200:**
```json
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
  "totalCount": 5, "pageNumber": 1, "pageSize": 20, "totalPages": 1,
  "hasNextPage": false, "hasPreviousPage": false
}
```

---

### Obtener cliente por ID

```
GET /api/Clients/{id}
```

**Respuesta 200:** objeto ClientDto.  
**Respuesta 404:** si no existe.

---

### Crear cliente — Admin o Analyst

```
POST /api/Clients
```

**Body:**
```json
{
  "name": "Empresa S.A.",
  "email": "contacto@empresa.com",
  "phone": "+1234567890",
  "company": "Empresa S.A.",
  "notes": "Opcional"
}
```

| Campo | Tipo | Requerido |
|---|---|---|
| `name` | string | ✅ |
| `email` | string | ✅ |
| `phone` | string | ❌ |
| `company` | string | ❌ |
| `notes` | string | ❌ |

**Respuesta 201:** `{ "data": "3fa85f64-..." }` (Guid del cliente)

---

### Actualizar cliente — Admin o Analyst

```
PUT /api/Clients/{id}
```

**Body:** `name`, `phone`, `company`, `notes`.  
**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

### Eliminar cliente — solo Admin

```
DELETE /api/Clients/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

## Projects 🔒

### Listar proyectos

```
GET /api/Projects?pageNumber=1&pageSize=20
```

**Respuesta 200:**
```json
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
  "totalCount": 2, "pageNumber": 1, "pageSize": 20, "totalPages": 1,
  "hasNextPage": false, "hasPreviousPage": false
}
```

**Valores de `status`:** `"Planning"` | `"Active"` | `"OnHold"` | `"Completed"` | `"Cancelled"`

---

### Obtener proyecto por ID

```
GET /api/Projects/{id}
```

**Respuesta 200:** objeto ProjectDto.

---

### Crear proyecto — Admin o Analyst

```
POST /api/Projects
```

**Body:**
```json
{
  "title": "Web App 2026",
  "description": "Opcional",
  "clientId": "3fa85f64-...",
  "analystId": "3fa85f64-...",
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-12-31T00:00:00Z"
}
```

| Campo | Tipo | Requerido |
|---|---|---|
| `title` | string | ✅ |
| `description` | string | ❌ |
| `clientId` | Guid | ✅ |
| `analystId` | Guid | ✅ |
| `startDate` | ISO 8601 | ❌ |
| `endDate` | ISO 8601 | ❌ |

**Respuesta 201:** `{ "data": "3fa85f64-..." }` (Guid del proyecto)

---

### Actualizar proyecto — Admin o Analyst

```
PUT /api/Projects/{id}
```

**Body:** `title`, `description`, `status`, `startDate`, `endDate`.  
**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

### Eliminar proyecto — solo Admin

```
DELETE /api/Projects/{id}
```

**Respuesta 200:** `{ "success": true, "data": null, ... }`

---

## Ejemplos con curl

```bash
# Registrar
curl -X POST http://localhost:8080/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Juan","lastName":"Pérez","email":"juan@example.com","password":"Segura1234!"}'

# Login
curl -X POST http://localhost:8080/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"juan@example.com","password":"Segura1234!"}'

# Listar boards (reemplaza TOKEN)
curl http://localhost:8080/api/Boards \
  -H "Authorization: Bearer TOKEN"

# Crear board
curl -X POST http://localhost:8080/api/Boards \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Mi Board","description":"Descripción"}'

# Crear tarea (reemplaza BOARD_ID)
curl -X POST "http://localhost:8080/api/Tasks?boardId=BOARD_ID" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Nueva tarea","priority":"Medium"}'

# Listar tareas con cursor (primera página)
curl "http://localhost:8080/api/Tasks/cursor?boardId=BOARD_ID&pageSize=20" \
  -H "Authorization: Bearer TOKEN"

# Siguiente página con cursor
curl "http://localhost:8080/api/Tasks/cursor?boardId=BOARD_ID&pageSize=20&cursor=NEXT_CURSOR" \
  -H "Authorization: Bearer TOKEN"

# Añadir comentario a una tarea
curl -X POST "http://localhost:8080/api/Tasks/TASK_ID/comments" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"content":"Mi comentario"}'

# Listar tags
curl http://localhost:8080/api/Tags \
  -H "Authorization: Bearer TOKEN"

# Crear tag (Admin)
curl -X POST http://localhost:8080/api/Tags \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Bug","color":"#EF4444"}'

# Asociar tag a tarea
curl -X POST "http://localhost:8080/api/Tasks/TASK_ID/tags/TAG_ID" \
  -H "Authorization: Bearer TOKEN"

# Ver mi perfil
curl http://localhost:8080/api/Users/me \
  -H "Authorization: Bearer TOKEN"

# Cambiar rol de usuario (Admin)
curl -X PATCH "http://localhost:8080/api/Users/USER_ID/role" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"role":2}'
```
