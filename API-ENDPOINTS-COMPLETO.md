# 🌐 TaskFlow API - Endpoints Completos para Next.js

> Referencia completa de todos los endpoints del backend C# para integrar en tu proyecto Next.js

**Base URL:** `http://localhost:8080/api`  
**Swagger:** `http://localhost:8080/swagger`

---

## 📋 Tabla de Contenidos

1. [Autenticación](#-autenticación)
2. [Usuarios](#-usuarios)
3. [Boards](#-boards)
4. [Tasks (Tareas)](#-tasks-tareas)
5. [Comments (Comentarios)](#-comments-comentarios)
6. [Tags (Etiquetas)](#-tags-etiquetas)
7. [Clients (Clientes)](#-clients-clientes)
8. [Projects (Proyectos)](#-projects-proyectos)

---

## 🔐 Autenticación

### 1. Registrar Usuario
```http
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

**Validaciones de contraseña:**
- Mínimo 8 caracteres
- Al menos una mayúscula
- Al menos una minúscula
- Al menos un número
- Al menos un símbolo especial

**Respuesta 201:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "accessTokenExpiresAt": "2026-06-01T12:15:00Z",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "juan@example.com",
    "firstName": "Juan",
    "lastName": "Pérez",
    "role": 0
  },
  "message": "Account created successfully.",
  "errors": null
}
```

**Roles:**
- `0` = Member (Miembro básico)
- `1` = Admin (Administrador - acceso total)
- `2` = Analyst (Analista - gestiona clientes y proyectos)
- `3` = Client (Cliente - solo lectura en proyectos asignados)

**Duraciones:**
- `accessToken`: **15 minutos**
- `refreshToken`: **7 días**

---

### 2. Iniciar Sesión
```http
POST /api/Auth/login
```

**Body:**
```json
{
  "email": "juan@example.com",
  "password": "Segura1234!"
}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "accessTokenExpiresAt": "2026-06-01T12:15:00Z",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "juan@example.com",
    "firstName": "Juan",
    "lastName": "Pérez",
    "role": 0
  },
  "message": "OK",
  "errors": null
}
```

---

### 3. Refrescar Token
```http
POST /api/Auth/refresh
```

**Body:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc123..."
}
```

> Enviar AMBOS tokens. El backend valida que ambos coincidan con la sesión activa.

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "nuevo_token...",
    "refreshToken": "nuevo_refresh...",
    "accessTokenExpiresAt": "2026-06-01T12:30:00Z",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "juan@example.com",
    "firstName": "Juan",
    "lastName": "Pérez",
    "role": 0
  },
  "message": "OK",
  "errors": null
}
```

**Respuesta 401:** Si el refreshToken expiró o es inválido → Redirigir a login

> **Sin body.** Invalida el refreshToken en la base de datos.

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Logged out successfully.",
  "errors": null
}
```

**Después del logout:**
- Limpiar el localStorage/sessionStorage
- Limpiar el caché de React Query
- Redirigir a `/loginRespuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Logged out successfully.",
  "errors": null
}
```

---

## 👤 Usuarios

### 1. Obtener Perfil Propio
```http
GET /api/Users/me
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Juan",
    "lastName": "Pérez",
    "fullName": "Juan Pérez",
    "email": "juan@example.com",
    "role": 0,
    "createdAt": "2026-06-01T10:00:00Z",
    "updatedAt": "2026-06-01T10:00:00Z"
  },
  "message": "OK",
  "errors": null
}
```

---

### 2. Listar Usuarios (Solo Admin)
```http
GET /api/Users?pageNumber=1&pageSize=20
Authorization: Bearer {token}
Roles: Admin
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "...",
        "firstName": "Juan",
        "lastName": "Pérez",
        "fullName": "Juan Pérez",
        "email": "juan@example.com",
        "role": 0,
        "createdAt": "2026-06-01T10:00:00Z",
        "updatedAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

---

### 3. Obtener Usuario por ID (Solo Admin)
```http
GET /api/Users/{id}
Authorization: Bearer {token}
Roles: Admin
```

---

### 4. Actualizar Perfil Propio
```http
PUT /api/Users/me
Authorization: Bearer {token}
```

**Body:**
```json
{
  "firstName": "Juan Carlos",
  "lastName": "Pérez Gómez"
}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Profile updated successfully.",
  "errors": null
}
```

---

### 5. Cambiar Rol de Usuario (Solo Admin)
```http
PATCH /api/Users/{id}/role
Authorization: Bearer {token}
Roles: Admin
```

**Body:**
```json
{
  "role": 1
}
```

---

### 6. Eliminar Usuario (Solo Admin)
```http
DELETE /api/Users/{id}
Authorization: Bearer {token}
Roles: Admin
```

---

## 📋 Boards

### 1. Listar Boards
```http
GET /api/Boards?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

> **Importante:** Solo retorna los boards del usuario autenticado (filtra por `OwnerId`).

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | número | 1 | Número de página |
| `pageSize` | número | 20 | Resultados por página |

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Proyecto Web",
        "description": "Board del proyecto web",
        "ownerId": "...",
        "taskCount": 15,
        "createdAt": "2026-06-01T10:00:00Z",
        "updatedAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

> **Nota:** El campo `projectId` (opcional, puede ser `null`) existe en la BD pero NO se incluye en la respuesta del DTO actual. Si necesitas asociar boards a proyectos, se hará en una futura versión del DTO.

---

### 2. Obtener Board por ID
```http
GET /api/Boards/{id}
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Proyecto Web",
    "description": "Board del proyecto web",
    "ownerId": "...",
    "taskCount": 15,
    "createdAt": "2026-06-01T10:00:00Z",
    "updatedAt": "2026-06-01T10:00:00Z"
  },
  "message": "OK",
  "errors": null
}
```

---

### 3. Crear Board
```http
POST /api/Boards
Authorization: Bearer {token}
```

**Body:**
```json
{
  "title": "Nuevo Board",
  "description": "Descripción opcional"
}
```

**Validaciones:**
- `title`: Requerido, máximo 200 caracteres
- `description`: Opcional, máximo 1000 caracteres

**Respuesta 201:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "OK",
  "errors": null
}
```

> El `data` es el ID (Guid) del board creado. El `ownerId` se asigna automáticamente con el usuario autenticado.

---

### 4. Actualizar Board
```http
PUT /api/Boards/{id}
Authorization: Bearer {token}
```

**Body:**
```json
{
  "title": "Board Actualizado",
  "description": "Nueva descripción"
}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Board updated successfully.",
  "errors": null
}
```

---

### 5. Eliminar Board
```http
DELETE /api/Boards/{id}
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
**Respuesta 404:** Board no existe  
**Respuesta 403:** No eres el dueño del board

> **Soft-delete:** No se borra físicamente de la BD. Se marca `IsDeleted = true`, `DeletedAt = UTC.Now`, `DeletedBy = userId`. Las tareas del board también se marcan como eliminadas en cascada (soft-delete).

  "data": null,
  "message": "Board deleted successfully.",
  "errors": null
}
```

---

## ✅ Tasks (Tareas)

### 1. Listar Tareas por Board (Paginación Offset)
```http
GET /api/Tasks?boardId={boardId}&pageNumber=1&pageSize=50
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Diseñar interfaz",
        "description": "Crear mockups del dashboard",
        "status": "InProgress",
        "priority": "High",
        "dueDate": "2026-06-15T00:00:00Z",
        "boardId": "...",
        "assigneeId": "...",
        "assigneeName": "Juan Pérez",
        "createdAt": "2026-06-01T10:00:00Z",
        "updatedAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 50,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

**Status:** `Todo`, `InProgress`, `Done`, `Cancelled`  
**Priority:** `Low`, `Medium`, `High`, `Critical`

---

### 2. Listar Tareas por Board (Paginación Cursor - Scroll Infinito)
```http
GET /api/Tasks/cursor?boardId={boardId}&cursor={cursor}&pageSize=50
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "nextCursor": "eyJpZCI6IjNmYTg1ZjY0LTU3MTctNDU2Mi1iM2ZjLTJjOTYzZjY2YWZhNiJ9",
    "hasNextPage": true,
    "pageSize": 50
  },
  "message": "OK",
  "errors": null
}
```

> Usa `nextCursor` en la siguiente petición para obtener más resultados.

---

### 3. Obtener Tarea por ID
```http
GET /api/Tasks/{id}
Authorization: Bearer {token}
```

---

### 4. Crear Tarea
```http
POST /api/Tasks?boardId={boardId}
Authorization: Bearer {token}
```

**Body:**
```json
{
  "title": "Nueva tarea",
  "description": "Descripción detallada",
  "priority": "High",
  "dueDate": "2026-06-15T00:00:00Z",
  "assigneeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Respuesta 201:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "OK",
  "errors": null
}
```

---

### 5. Actualizar Tarea
```http
PUT /api/Tasks/{id}
Authorization: Bearer {token}
```

**Body:**
```json
{
  "title": "Tarea actualizada",
  "description": "Nueva descripción",
  "status": "InProgress",
  "priority": "Critical",
  "dueDate": "2026-06-20T00:00:00Z",
  "assigneeId": "..."
}
```

---

### 6. Eliminar Tarea
```http
DELETE /api/Tasks/{id}
Authorization: Bearer {token}
```

---

## 💬 Comments (Comentarios)

### 1. Listar Comentarios de una Tarea
```http
GET /api/Tasks/{taskId}/comments?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "content": "Este diseño se ve muy bien",
        "taskItemId": "...",
        "authorId": "...",
        "authorFullName": "Juan Pérez",
        "createdAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

---

### 2. Agregar Comentario
```http
POST /api/Tasks/{taskId}/comments
Authorization: Bearer {token}
```

**Body:**
```json
{
  "content": "Este es mi comentario"
}
```

**Respuesta 201:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "OK",
  "errors": null
}
```

---

### 3. Eliminar Comentario
```http
DELETE /api/Tasks/{taskId}/comments/{commentId}
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Comment deleted successfully.",
  "errors": null
}
```

---

## 🏷️ Tags (Etiquetas)

### 1. Listar Todos los Tags
```http
GET /api/Tags
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Urgente",
      "color": "#FF0000"
    },
    {
      "id": "...",
      "name": "Bug",
      "color": "#FFA500"
    }
  ],
  "message": "OK",
  "errors": null
}
```

---

### 2. Crear Tag (Solo Admin)
```http
POST /api/Tags
Authorization: Bearer {token}
Roles: Admin
```

**Body:**
```json
{
  "name": "Nuevo Tag",
  "color": "#00FF00"
}
```

**Respuesta 201:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "OK",
  "errors": null
}
```

---

### 3. Eliminar Tag (Solo Admin)
```http
DELETE /api/Tags/{id}
Authorization: Bearer {token}
Roles: Admin
```

---

### 4. Agregar Tag a Tarea
```http
POST /api/tasks/{taskId}/tags/{tagId}
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Tag added to task.",
  "errors": null
}
```

---

### 5. Quitar Tag de Tarea
```http
DELETE /api/tasks/{taskId}/tags/{tagId}
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": null,
  "message": "Tag removed from task.",
  "errors": null
}
```

---

## 👥 Clients (Clientes)

### 1. Listar Clientes
```http
GET /api/Clients?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Empresa XYZ",
        "email": "contacto@empresa.com",
        "phone": "+52 55 1234 5678",
        "company": "Empresa XYZ S.A.",
        "notes": "Cliente importante",
        "projectCount": 3,
        "createdAt": "2026-06-01T10:00:00Z",
        "updatedAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

---

### 2. Obtener Cliente por ID
```http
GET /api/Clients/{id}
Authorization: Bearer {token}
```

---

### 3. Crear Cliente (Solo Admin/Analyst)
```http
POST /api/Clients
Authorization: Bearer {token}
Roles: Admin, Analyst
```

**Body:**
```json
{
  "name": "Nuevo Cliente",
  "email": "cliente@example.com",
  "phone": "+52 55 1234 5678",
  "company": "Empresa ABC",
  "notes": "Notas adicionales"
}
```

---

### 4. Actualizar Cliente (Solo Admin/Analyst)
```http
PUT /api/Clients/{id}
Authorization: Bearer {token}
Roles: Admin, Analyst
```

**Body:**
```json
{
  "name": "Cliente Actualizado",
  "phone": "+52 55 9876 5432",
  "company": "Nueva Empresa",
  "notes": "Notas actualizadas"
}
```

---

### 5. Eliminar Cliente (Solo Admin)
```http
DELETE /api/Clients/{id}
Authorization: Bearer {token}
Roles: Admin
```

---

## 📁 Projects (Proyectos)

### 1. Listar Proyectos
```http
GET /api/Projects?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Sitio Web Corporativo",
        "description": "Desarrollo del sitio web principal",
        "status": "Active",
        "startDate": "2026-06-01T00:00:00Z",
        "endDate": "2026-12-31T00:00:00Z",
        "clientId": "...",
        "clientName": "Empresa XYZ",
        "analystId": "...",
        "analystFullName": "Juan Pérez",
        "boardCount": 2,
        "createdAt": "2026-06-01T10:00:00Z",
        "updatedAt": "2026-06-01T10:00:00Z"
      }
    ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "OK",
  "errors": null
}
```

**Status:** `Planning`, `Active`, `OnHold`, `Completed`, `Cancelled`

---

### 2. Obtener Proyecto por ID
```http
GET /api/Projects/{id}
Authorization: Bearer {token}
```

---

### 3. Crear Proyecto (Solo Admin/Analyst)
```http
POST /api/Projects
Authorization: Bearer {token}
Roles: Admin, Analyst
```

**Body:**
```json
{
  "title": "Nuevo Proyecto",
  "description": "Descripción del proyecto",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "analystId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-12-31T00:00:00Z"
}
```

**Respuesta 201:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "OK",
  "errors": null
}
```

---

### 4. Actualizar Proyecto (Solo Admin/Analyst)
```http
PUT /api/Projects/{id}
Authorization: Bearer {token}
Roles: Admin, Analyst
```

**Body:**
```json
{
  "title": "Proyecto Actualizado",
  "description": "Nueva descripción",
  "status": "Active",
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-12-31T00:00:00Z"
}
```

---

### 5. Eliminar Proyecto (Solo Admin)
```http
DELETE /api/Projects/{id}
Authorization: Bearer {token}
Roles: Admin
```

---

## 🔑 Autenticación en Next.js

### Configuración de Axios

```typescript
// src/lib/api.ts
import axios from 'axios';
import { useAuthStore } from '@/store/auth.store';

export const api = axios.create({
  baseURL: 'http://localhost:8080/api',
});

// Interceptor: Agregar token automáticamente
api.interceptors.request.use((config) => {
  const user = useAuthStore.getState().user;
  if (user?.accessToken) {
    config.headers.Authorization = `Bearer ${user.accessToken}`;
  }
  return config;
});
```

---

## 📊 Formato de Respuesta

Todas las respuestas siguen el mismo formato:

### Respuesta Exitosa
```json
{
  "success": true,
  "data": { ... },
  "message": "OK",
  "errors": null
}
```

### Respuesta de Error
```json
{
  "success": false,
  "data": null,
  "message": "Descripción del error",
  "errors": ["Detalle 1", "Detalle 2"]
}
```

---

## 🚨 Códigos HTTP

| Código | Significado |
|--------|-------------|
| `200` | OK - Operación exitosa |
| `201` | Created - Recurso creado |
| `400` | Bad Request - Datos inválidos |
| `401` | Unauthorized - Token inválido o expirado |
| `403` | Forbidden - No tienes permiso |
| `404` | Not Found - Recurso no encontrado |
| `409` | Conflict - Email ya existe, etc. |
| `500` | Internal Server Error |

---

## 🔒 Permisos por Rol

| Recurso | Member (0) | Admin (1) | Analyst (2) | Client (3) |
|---------|-----------|-----------|-------------|-----------|
| **Auth** | ✅ Todos | ✅ Todos | ✅ Todos | ✅ Todos |
| **Boards** | ✅ CRUD propios | ✅ CRUD todos | ✅ CRUD propios | ❌ Solo lectura |
| **Tasks** | ✅ CRUD propios | ✅ CRUD todos | ✅ CRUD propios | ❌ Solo lectura |
| **Users** | ✅ Ver/Editar perfil | ✅ CRUD todos | ✅ Ver/Editar perfil | ✅ Ver/Editar perfil |
| **Tags** | ✅ Leer, Asociar | ✅ CRUD todos | ✅ Leer, Asociar | ❌ Solo lectura |
| **Clients** | ✅ Leer | ✅ CRUD | ✅ CRUD | ❌ Ver propios |
| **Projects** | ✅ Leer | ✅ CRUD | ✅ CRUD | ❌ Ver propios |

---

## 📝 Resumen de Endpoints

### Total: **45 Endpoints**

- **Auth:** 4 endpoints
- **Users:** 6 endpoints
- **Boards:** 5 endpoints
- **Tasks:** 6 endpoints
- **Comments:** 3 endpoints
- **Tags:** 5 endpoints
- **Clients:** 5 endpoints
- **Projects:** 5 endpoints
- **Task-Tags:** 2 endpoints (relación N:M)

---

## 🚀 Uso en Next.js

### Ejemplo: Hook personalizado

```typescript
// src/hooks/useTasks.ts
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';

export function useTasks(boardId: string, page = 1) {
  return useQuery({
    queryKey: ['tasks', boardId, page],
    queryFn: async () => {
      const response = await api.get('/Tasks', {
        params: { boardId, pageNumber: page, pageSize: 50 }
      });
      return response.data.data;
    },
    enabled: !!boardId,
  });
}

// Uso en componente
const { data, isLoading } = useTasks(boardId, 1);
```

---

**Última actualización:** 1 de junio de 2026
