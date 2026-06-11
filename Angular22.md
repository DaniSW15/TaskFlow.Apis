# Guía de integración Angular v22 + TaskFlow API

Esta guía cubre cómo consumir la TaskFlow API desde una aplicación Angular v22 utilizando las últimas características del framework: **Signals**, **rxResource** (para carga reactiva de datos), **Signal Inputs/Outputs**, **Model Inputs** (para enlace bidireccional), **Route Component Input Binding** e **interceptores funcionales**.

---

## Referencia completa de la API

> **Base URL local:** `http://localhost:8080/api`  
> **Base URL producción:** `https://tu-dominio.com/api`  
> **Formato:** JSON. Todos los endpoints devuelven el wrapper `ApiResponse<T>`.

### Wrapper de respuesta estándar
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

## Requisitos previos y Setup de Angular v22

Para inicializar tu aplicación de Angular v22 lista para usar TypeScript estricto, enrutamiento y preprocesador SCSS:

```bash
# Verificar versiones (Node 22+, Angular CLI 19/20/22)
node -v
ng version

# Crear nueva aplicación standalone
ng new taskflow-frontend --standalone --routing --style=scss --strict
cd taskflow-frontend
```

---

## 1. Configuración de Entornos y Aplicación

### Environments
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080/api'
};

// src/environments/environment.production.ts
export const environment = {
  production: true,
  apiUrl: 'https://taskflow-apis.onrender.com/api'
};
```

### Configuración de la App (`app.config.ts`)
Para habilitar el enlace automático de parámetros de ruta a inputs de componentes (`withComponentInputBinding`) y registrar el interceptor de autenticación:

```typescript
// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

---

## 2. Modelos de Datos (`src/app/core/models/`)

### `auth.models.ts`
```typescript
export enum UserRole {
  Member = 0,
  Admin = 1,
  Analyst = 2,
  Client = 3
}

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
```

### `api.models.ts`
```typescript
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[] | null;
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
```

### `board.models.ts`
```typescript
export interface Board {
  id: string;
  title: string;
  description?: string;
  ownerId: string;
  taskCount: number;
  createdAt: string;
  updatedAt: string;
}
```

### `task.models.ts`
```typescript
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
```

### `comment.models.ts`
```typescript
export interface Comment {
  id: string;
  content: string;
  taskItemId: string;
  authorId: string;
  authorFullName: string;
  createdAt: string;
}
```

### `tag.models.ts`
```typescript
export interface Tag {
  id: string;
  name: string;
  color: string;
}
```

### `client.models.ts`
```typescript
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

### `project.models.ts`
```typescript
export enum ProjectStatus {
  Planning = 'Planning',
  Active = 'Active',
  OnHold = 'OnHold',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

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
```

### `user.models.ts`
```typescript
import { UserRole } from './auth.models';

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

---

## 3. Servicios e Interceptores Funcionales

### `AuthService` (`src/app/core/services/auth.service.ts`)
Implementado usando **Signals** nativas de lectura y escritura para reaccionar globalmente al estado de autenticación.

```typescript
import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError, EMPTY } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '../models/auth.models';
import { ApiResponse } from '../models/api.models';

const ACCESS_TOKEN_KEY = 'tf_access_token';
const REFRESH_TOKEN_KEY = 'tf_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  // --- Signals de Estado ---
  private readonly _user = signal<AuthResponse | null>(this.loadStoredUser());

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.role === UserRole.Admin);
  readonly isAnalyst = computed(() => this._user()?.role === UserRole.Analyst);
  readonly isAdminOrAnalyst = computed(() =>
    this._user()?.role === UserRole.Admin || this._user()?.role === UserRole.Analyst
  );
  readonly accessToken = computed(() => this._user()?.accessToken ?? null);

  register(request: RegisterRequest) {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  login(request: LoginRequest) {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  logout() {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/logout`, {}).pipe(
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

    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/refresh`, {
      accessToken: at,
      refreshToken: rt
    }).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

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
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!token || !refreshToken) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000;
      if (Date.now() > exp) return null; // Token expirado; se renueva vía interceptor al hacer peticiones

      // Reconstruir objeto mínimo
      return {
        accessToken: token,
        refreshToken: refreshToken,
        accessTokenExpiresAt: new Date(exp).toISOString(),
        userId: payload.nameid || payload.sub || '',
        email: payload.email || '',
        firstName: payload.firstName || '',
        lastName: payload.lastName || '',
        role: Number(payload.role) as UserRole
      };
    } catch {
      return null;
    }
  }
}
```

---

### Interceptor de Autenticación (`src/app/core/interceptors/auth.interceptor.ts`)
Interceptor funcional que inyecta automáticamente el token Bearer en las cabeceras. Si recibe un error `401 Unauthorized`, pausa las peticiones, refresca el token JWT y reintenta la petición original.

```typescript
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/Auth/refresh')) {
        return auth.refreshToken().pipe(
          switchMap((res) => {
            const retriedReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.data.accessToken}` }
            });
            return next(retriedReq);
          }),
          catchError((refreshErr) => {
            auth.logout(); // Si falla el refresh, limpia la sesión
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
```

---

### Guardas Funcionales (`src/app/core/guards/`)

```typescript
// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

// src/app/core/guards/admin.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAdmin() ? true : router.createUrlTree(['/boards']);
};
```

---

## 4. Servicios de Negocio (`src/app/core/services/`)

### `BoardsService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/api.models';
import { Board } from '../models/board.models';

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

### `TasksService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList, CursorPaginatedList } from '../models/api.models';
import { TaskItem, TaskItemStatus, TaskPriority } from '../models/task.models';
import { Comment } from '../models/comment.models';

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

  getTasksByBoardWithCursor(boardId: string, pageSize = 20, cursor?: string) {
    let params = new HttpParams().set('boardId', boardId).set('pageSize', pageSize);
    if (cursor) params = params.set('cursor', cursor);
    return this.http.get<ApiResponse<CursorPaginatedList<TaskItem>>>(`${this.baseUrl}/cursor`, { params });
  }

  getTaskById(id: string) {
    return this.http.get<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`);
  }

  createTask(boardId: string, task: Partial<TaskItem>) {
    const params = new HttpParams().set('boardId', boardId);
    return this.http.post<ApiResponse<string>>(this.baseUrl, task, { params });
  }

  updateTask(id: string, task: Partial<TaskItem>) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, task);
  }

  deleteTask(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }

  // --- Sub-recurso: Comments ---
  getComments(taskId: string, pageNumber = 1, pageSize = 20) {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Comment>>>(`${this.baseUrl}/${taskId}/comments`, { params });
  }

  addComment(taskId: string, content: string) {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/${taskId}/comments`, { content });
  }

  deleteComment(taskId: string, commentId: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${taskId}/comments/${commentId}`);
  }

  // --- Relación N:M: Tags ---
  addTag(taskId: string, tagId: string) {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/${taskId}/tags/${tagId}`, {});
  }

  removeTag(taskId: string, tagId: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${taskId}/tags/${tagId}`);
  }
}
```

### `TagsService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { Tag } from '../models/tag.models';

@Injectable({ providedIn: 'root' })
export class TagsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tags`;

  getTags() {
    return this.http.get<ApiResponse<Tag[]>>(this.baseUrl);
  }

  createTag(name: string, color: string) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, { name, color });
  }

  deleteTag(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

### `UsersService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/api.models';
import { User } from '../models/user.models';
import { UserRole } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Users`;

  getUsers(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
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

### `ClientsService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/api.models';
import { Client } from '../models/client.models';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Clients`;

  getClients(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Client>>>(this.baseUrl, { params });
  }

  getClientById(id: string) {
    return this.http.get<ApiResponse<Client>>(`${this.baseUrl}/${id}`);
  }

  createClient(client: Partial<Client>) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, client);
  }

  updateClient(id: string, client: Partial<Client>) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, client);
  }

  deleteClient(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

### `ProjectsService`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedList } from '../models/api.models';
import { Project } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Projects`;

  getProjects(pageNumber = 1, pageSize = 20) {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PaginatedList<Project>>>(this.baseUrl, { params });
  }

  getProjectById(id: string) {
    return this.http.get<ApiResponse<Project>>(`${this.baseUrl}/${id}`);
  }

  createProject(project: Partial<Project>) {
    return this.http.post<ApiResponse<string>>(this.baseUrl, project);
  }

  updateProject(id: string, project: Partial<Project>) {
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}`, project);
  }

  deleteProject(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 5. Ruteo Global (`src/app/app.routes.ts`)

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    loadComponent: () => import('./features/layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'boards',
        loadComponent: () => import('./features/boards/boards-list/boards-list.component').then(m => m.BoardsListComponent)
      },
      {
        path: 'boards/:id',
        loadComponent: () => import('./features/boards/board-detail/board-detail.component').then(m => m.BoardDetailComponent)
      },
      {
        path: 'clients',
        loadComponent: () => import('./features/clients/clients-list.component').then(m => m.ClientsListComponent)
      },
      {
        path: 'projects',
        loadComponent: () => import('./features/projects/projects-list.component').then(m => m.ProjectsListComponent)
      },
      {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/admin.component').then(m => m.AdminComponent)
      },
      { path: '', redirectTo: 'boards', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
```

---

## 6. Componentes Standalone Modernos

### 6.1 `MainLayoutComponent` (Layout Base con Navbar)
Contiene la estructura visual principal del dashboard con validaciones de visibilidad de enlaces por rol y cierre de sesión.

```typescript
// src/app/features/layout/main-layout.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="layout-container">
      <nav class="navbar">
        <div class="nav-brand">
          <a routerLink="/boards">TaskFlow v22</a>
        </div>
        <ul class="nav-links">
          <li><a routerLink="/boards" routerLinkActive="active">Tableros</a></li>
          <li><a routerLink="/projects" routerLinkActive="active">Proyectos</a></li>
          <li><a routerLink="/clients" routerLinkActive="active">Clientes</a></li>
          @if (auth.isAdmin()) {
            <li><a routerLink="/admin" routerLinkActive="active">Admin Panel</a></li>
          }
        </ul>
        <div class="nav-user">
          <span class="user-name">{{ auth.user()?.firstName }} {{ auth.user()?.lastName }}</span>
          <span class="user-role badge-role">{{ getRoleName(auth.user()?.role) }}</span>
          <button (click)="onLogout()" class="logout-btn">Salir</button>
        </div>
      </nav>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .layout-container { display: flex; flex-direction: column; min-height: 100vh; font-family: 'Outfit', sans-serif; background: #f8fafc; }
    .navbar { display: flex; justify-content: space-between; align-items: center; padding: 1rem 2rem; background: #0f172a; color: white; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1); }
    .nav-brand a { font-size: 1.5rem; font-weight: 700; color: #38bdf8; text-decoration: none; }
    .nav-links { display: flex; list-style: none; gap: 1.5rem; margin: 0; padding: 0; }
    .nav-links a { color: #94a3b8; text-decoration: none; font-weight: 500; transition: color 0.2s; }
    .nav-links a:hover, .nav-links a.active { color: white; }
    .nav-user { display: flex; align-items: center; gap: 1rem; }
    .badge-role { background: #334155; padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; color: #cbd5e1; }
    .logout-btn { background: #ef4444; border: none; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-weight: 600; cursor: pointer; transition: background 0.2s; }
    .logout-btn:hover { background: #dc2626; }
    .content { flex: 1; padding: 2rem; max-width: 1400px; width: 100%; margin: 0 auto; box-sizing: border-box; }
  `]
})
export class MainLayoutComponent {
  protected readonly auth = inject(AuthService);

  getRoleName(role: number | undefined): string {
    if (role === undefined) return '';
    return ['Member', 'Admin', 'Analyst', 'Client'][role] || '';
  }

  onLogout() {
    this.auth.logout().subscribe();
  }
}
```

---

### 6.2 `BoardsListComponent` (Uso de `rxResource` y Modales)
Implementa el uso de `rxResource` para la lectura reactiva ligada al signal de la página activa. Utiliza enlaces modernos y modales con `model()`.

```typescript
// src/app/features/boards/boards-list/boards-list.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { BoardsService } from '../../../core/services/boards.service';
import { Board } from '../../../core/models/board.models';

@Component({
  selector: 'app-boards-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  template: `
    <div class="boards-container">
      <div class="header">
        <h1>Mis Tableros</h1>
        <button (click)="showCreateModal.set(true)" class="btn-primary">+ Nuevo Tablero</button>
      </div>

      <!-- rxResource: Estado de Carga -->
      @if (boardsResource.isLoading()) {
        <div class="loading">Cargando tableros...</div>
      } @else if (boardsResource.error()) {
        <div class="error-banner">Ha ocurrido un error al cargar los datos.</div>
      } @else {
        <div class="boards-grid">
          @for (board of boardsResource.value()?.items; track board.id) {
            <div class="board-card">
              <div [routerLink]="['/boards', board.id]" class="card-content">
                <h3>{{ board.title }}</h3>
                <p>{{ board.description || 'Sin descripción' }}</p>
                <span class="count">{{ board.taskCount }} tareas</span>
              </div>
              <div class="card-actions">
                <button (click)="openEdit(board)" class="btn-sec">Editar</button>
                <button (click)="deleteBoard(board.id)" class="btn-danger">Eliminar</button>
              </div>
            </div>
          } @empty {
            <div class="empty-state">No tienes tableros asignados. ¡Crea uno nuevo!</div>
          }
        </div>

        <!-- Paginación -->
        @if (boardsResource.value(); as data) {
          <div class="pagination">
            <button (click)="changePage(-1)" [disabled]="!data.hasPreviousPage" class="btn-page">Anterior</button>
            <span>Página {{ currentPage() }} de {{ data.totalPages }}</span>
            <button (click)="changePage(1)" [disabled]="!data.hasNextPage" class="btn-page">Siguiente</button>
          </div>
        }
      }

      <!-- Modal Crear/Editar -->
      @if (showCreateModal() || editingBoard()) {
        <div class="modal-overlay" (click)="closeModals()">
          <div class="modal-card" (click)="$event.stopPropagation()">
            <h2>{{ editingBoard() ? 'Editar' : 'Crear' }} Tablero</h2>
            <form [formGroup]="boardForm" (ngSubmit)="onSubmit()">
              <div class="form-group">
                <label>Título *</label>
                <input formControlName="title" placeholder="Ej: Rediseño Web" />
              </div>
              <div class="form-group">
                <label>Descripción</label>
                <textarea formControlName="description" rows="3" placeholder="Detalles..."></textarea>
              </div>
              <div class="modal-actions">
                <button type="button" (click)="closeModals()" class="btn-sec">Cancelar</button>
                <button type="submit" [disabled]="boardForm.invalid || saving()" class="btn-primary">
                  {{ saving() ? 'Guardando...' : 'Guardar' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
    .boards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1.5rem; }
    .board-card { background: white; border-radius: 0.75rem; border: 1px solid #e2e8f0; display: flex; flex-direction: column; overflow: hidden; transition: transform 0.2s, box-shadow 0.2s; }
    .board-card:hover { transform: translateY(-2px); box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.05); }
    .card-content { padding: 1.5rem; flex: 1; cursor: pointer; text-decoration: none; color: inherit; }
    .card-content h3 { margin: 0 0 0.5rem; font-size: 1.25rem; color: #1e293b; }
    .card-content p { color: #64748b; font-size: 0.875rem; margin: 0 0 1rem; }
    .card-content .count { font-size: 0.75rem; background: #e0f2fe; color: #0369a1; padding: 0.25rem 0.5rem; border-radius: 0.25rem; font-weight: 600; }
    .card-actions { display: flex; border-top: 1px solid #f1f5f9; background: #f8fafc; padding: 0.75rem 1.2rem; gap: 0.5rem; }
    .btn-primary { background: #3b82f6; color: white; border: none; padding: 0.6rem 1.2rem; border-radius: 0.375rem; font-weight: 600; cursor: pointer; }
    .btn-primary:hover { background: #2563eb; }
    .btn-sec { background: white; border: 1px solid #cbd5e1; padding: 0.5rem 1rem; border-radius: 0.375rem; cursor: pointer; font-weight: 500; }
    .btn-sec:hover { background: #f8fafc; }
    .btn-danger { background: #ef4444; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.375rem; cursor: pointer; font-weight: 500; }
    .btn-danger:hover { background: #dc2626; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 2.5rem; }
    .btn-page { background: white; border: 1px solid #cbd5e1; padding: 0.5rem 1rem; border-radius: 0.375rem; cursor: pointer; }
    .btn-page:disabled { opacity: 0.5; cursor: not-allowed; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.45); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 1000; }
    .modal-card { background: white; padding: 2rem; border-radius: 1rem; max-width: 500px; width: 100%; box-shadow: 0 20px 25px -5px rgb(0 0 0 / 0.1); }
    .modal-card h2 { margin-top: 0; }
    .form-group { margin-bottom: 1.25rem; display: flex; flex-direction: column; gap: 0.5rem; }
    .form-group input, .form-group textarea { padding: 0.6rem; border: 1px solid #cbd5e1; border-radius: 0.375rem; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }
  `]
})
export class BoardsListComponent {
  private readonly boardsService = inject(BoardsService);
  private readonly fb = inject(FormBuilder);

  readonly currentPage = signal(1);
  readonly showCreateModal = signal(false);
  readonly editingBoard = signal<Board | null>(null);
  readonly saving = signal(false);

  readonly boardForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  // --- Carga Reactiva con rxResource ---
  readonly boardsResource = rxResource({
    request: () => ({ page: this.currentPage() }),
    loader: ({ request }) => this.boardsService.getBoards(request.page, 20).pipe(
      map(res => res.data)
    )
  });

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
  }

  openEdit(board: Board) {
    this.editingBoard.set(board);
    this.boardForm.setValue({
      title: board.title,
      description: board.description ?? ''
    });
  }

  closeModals() {
    this.showCreateModal.set(false);
    this.editingBoard.set(null);
    this.boardForm.reset();
    this.saving.set(false);
  }

  onSubmit() {
    if (this.boardForm.invalid) return;
    this.saving.set(true);

    const { title, description } = this.boardForm.value;
    const board = this.editingBoard();

    const request$ = board
      ? this.boardsService.updateBoard(board.id, title!, description ?? undefined)
      : this.boardsService.createBoard(title!, description ?? undefined);

    request$.subscribe({
      next: () => {
        this.boardsResource.reload(); // Recarga reactiva
        this.closeModals();
      },
      error: () => this.saving.set(false)
    });
  }

  deleteBoard(id: string) {
    if (!confirm('¿Seguro que deseas eliminar este tablero y sus tareas correspondientes?')) return;
    this.boardsService.deleteBoard(id).subscribe(() => this.boardsResource.reload());
  }
}
```

---

### 6.3 `BoardDetailComponent` (Enlace de Inputs del Router)
Demuestra cómo enlazar parámetros de ruta (`id`) de manera nativa sin usar `ActivatedRoute`. Declara el ID como un signal input `readonly id = input.required<string>()`.

```typescript
// src/app/features/boards/board-detail/board-detail.component.ts
import { Component, inject, signal, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { BoardsService } from '../../../core/services/boards.service';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskItem, TaskItemStatus, TaskPriority } from '../../../core/models/task.models';
import { CommentsComponent } from '../comments/comments.component';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CommentsComponent],
  template: `
    <div class="detail-container">
      <!-- rxResource: Detalle del Tablero -->
      @if (boardResource.isLoading()) {
        <div>Cargando tablero...</div>
      } @else {
        <div class="board-header">
          <div>
            <h1>{{ boardResource.value()?.title }}</h1>
            <p>{{ boardResource.value()?.description || 'Sin descripción' }}</p>
          </div>
          <button (click)="openCreateTask()" class="btn-primary">+ Nueva Tarea</button>
        </div>
      }

      <hr class="divider"/>

      <!-- Grid de Tareas por Estado -->
      @if (tasksResource.isLoading()) {
        <div class="loading">Cargando tareas...</div>
      } @else {
        <div class="kanban-board">
          @for (column of columns; track column) {
            <div class="kanban-col">
              <h2>{{ column }}</h2>
              <div class="tasks-stack">
                @for (task of getTasksByStatus(column); track task.id) {
                  <div class="task-card">
                    <div class="task-info">
                      <h4>{{ task.title }}</h4>
                      <p>{{ task.description || 'Sin descripción' }}</p>
                      <div class="task-meta">
                        <span class="badge" [ngClass]="task.priority.toLowerCase()">{{ task.priority }}</span>
                        @if (task.dueDate) {
                          <span class="due">{{ task.dueDate | date:'dd/MM/yyyy' }}</span>
                        }
                      </div>
                    </div>
                    <div class="task-actions">
                      <button (click)="openComments(task)" class="btn-small">Comentarios</button>
                      <button (click)="openEditTask(task)" class="btn-small">Editar</button>
                      <button (click)="deleteTask(task.id)" class="btn-small danger">Borrar</button>
                    </div>
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }

      <!-- Modales de CRUD -->
      @if (showTaskModal()) {
        <div class="modal-overlay" (click)="closeTaskModal()">
          <div class="modal-card" (click)="$event.stopPropagation()">
            <h2>{{ editingTask() ? 'Editar' : 'Crear' }} Tarea</h2>
            <form [formGroup]="taskForm" (ngSubmit)="onSubmitTask()">
              <div class="form-group">
                <label>Título *</label>
                <input formControlName="title"/>
              </div>
              <div class="form-group">
                <label>Descripción</label>
                <textarea formControlName="description"></textarea>
              </div>
              <div class="form-row">
                <div class="form-group flex-1">
                  <label>Prioridad *</label>
                  <select formControlName="priority">
                    <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
                  </select>
                </div>
                @if (editingTask()) {
                  <div class="form-group flex-1">
                    <label>Estado *</label>
                    <select formControlName="status">
                      <option *ngFor="let s of statuses" [value]="s">{{ s }}</option>
                    </select>
                  </div>
                }
              </div>
              <div class="form-group">
                <label>Fecha Límite</label>
                <input formControlName="dueDate" type="date"/>
              </div>
              <div class="modal-actions">
                <button type="button" (click)="closeTaskModal()" class="btn-sec">Cancelar</button>
                <button type="submit" [disabled]="taskForm.invalid" class="btn-primary">Guardar</button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Drawer de Comentarios -->
      @if (activeTaskForComments()) {
        <div class="drawer-overlay" (click)="activeTaskForComments.set(null)">
          <div class="drawer" (click)="$event.stopPropagation()">
            <app-comments [taskId]="activeTaskForComments()!.id" (onClose)="activeTaskForComments.set(null)"/>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .board-header { display: flex; justify-content: space-between; align-items: center; }
    .divider { border: 0; border-top: 1px solid #e2e8f0; margin: 1.5rem 0; }
    .kanban-board { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; align-items: start; }
    .kanban-col { background: #f1f5f9; border-radius: 0.75rem; padding: 1rem; min-height: 500px; }
    .kanban-col h2 { font-size: 1rem; color: #475569; border-bottom: 2px solid #cbd5e1; padding-bottom: 0.5rem; text-transform: uppercase; margin-top: 0; }
    .tasks-stack { display: flex; flex-direction: column; gap: 0.75rem; margin-top: 1rem; }
    .task-card { background: white; border-radius: 0.5rem; padding: 1rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border-left: 4px solid #94a3b8; }
    .task-info h4 { margin: 0 0 0.5rem; color: #1e293b; }
    .task-info p { margin: 0 0 0.75rem; font-size: 0.875rem; color: #64748b; }
    .task-meta { display: flex; justify-content: space-between; align-items: center; font-size: 0.75rem; }
    .badge { padding: 0.15rem 0.4rem; border-radius: 0.25rem; font-weight: 600; text-transform: uppercase; }
    .badge.critical { background: #fee2e2; color: #991b1b; }
    .badge.high { background: #ffedd5; color: #9a3412; }
    .badge.medium { background: #fef9c3; color: #854d0e; }
    .badge.low { background: #f0fdf4; color: #166534; }
    .task-actions { display: flex; gap: 0.25rem; margin-top: 1rem; border-top: 1px solid #f1f5f9; padding-top: 0.5rem; justify-content: flex-end; }
    .btn-small { padding: 0.25rem 0.5rem; font-size: 0.75rem; border: 1px solid #cbd5e1; background: white; border-radius: 0.25rem; cursor: pointer; }
    .btn-small.danger { background: #fee2e2; color: #b91c1c; border-color: #fecaca; }
    .drawer-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.45); display: flex; justify-content: flex-end; z-index: 1000; }
    .drawer { background: white; width: 450px; height: 100%; box-shadow: -10px 0 20px rgba(0,0,0,0.1); padding: 2rem; display: flex; flex-direction: column; }
    .form-row { display: flex; gap: 1rem; }
    .flex-1 { flex: 1; }
  `]
})
export class BoardDetailComponent {
  private readonly boardsService = inject(BoardsService);
  private readonly tasksService = inject(TasksService);
  private readonly fb = inject(FormBuilder);

  // --- Parameter binding vía Signal Input ---
  readonly id = input.required<string>(); // Vinculado a la ruta 'boards/:id' automáticamente

  readonly showTaskModal = signal(false);
  readonly editingTask = signal<TaskItem | null>(null);
  readonly activeTaskForComments = signal<TaskItem | null>(null);

  readonly priorities = Object.values(TaskPriority);
  readonly statuses = Object.values(TaskItemStatus);
  readonly columns = Object.values(TaskItemStatus);

  readonly taskForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    priority: [TaskPriority.Medium, Validators.required],
    status: [TaskItemStatus.Todo],
    dueDate: ['']
  });

  // --- Resources reactivos basados en el ID de la ruta ---
  readonly boardResource = rxResource({
    request: () => ({ boardId: this.id() }),
    loader: ({ request }) => this.boardsService.getBoardById(request.boardId).pipe(
      map(res => res.data)
    )
  });

  readonly tasksResource = rxResource({
    request: () => ({ boardId: this.id() }),
    loader: ({ request }) => this.tasksService.getTasksByBoard(request.boardId, 1, 100).pipe(
      map(res => res.data.items)
    )
  });

  getTasksByStatus(status: TaskItemStatus): TaskItem[] {
    return (this.tasksResource.value() ?? []).filter(t => t.status === status);
  }

  openCreateTask() {
    this.editingTask.set(null);
    this.taskForm.reset({ priority: TaskPriority.Medium, status: TaskItemStatus.Todo });
    this.showTaskModal.set(true);
  }

  openEditTask(task: TaskItem) {
    this.editingTask.set(task);
    this.taskForm.setValue({
      title: task.title,
      description: task.description ?? '',
      priority: task.priority,
      status: task.status,
      dueDate: task.dueDate ? task.dueDate.substring(0, 10) : ''
    });
    this.showTaskModal.set(true);
  }

  closeTaskModal() {
    this.showTaskModal.set(false);
    this.editingTask.set(null);
  }

  onSubmitTask() {
    if (this.taskForm.invalid) return;

    const taskData = this.taskForm.value;
    const task = this.editingTask();

    const request$ = task
      ? this.tasksService.updateTask(task.id, taskData as Partial<TaskItem>)
      : this.tasksService.createTask(this.id(), taskData as Partial<TaskItem>);

    request$.subscribe(() => {
      this.tasksResource.reload();
      this.closeTaskModal();
    });
  }

  deleteTask(taskId: string) {
    if (!confirm('¿Eliminar esta tarea?')) return;
    this.tasksService.deleteTask(taskId).subscribe(() => this.tasksResource.reload());
  }

  openComments(task: TaskItem) {
    this.activeTaskForComments.set(task);
  }
}
```

---

### 6.4 `ClientsListComponent` (CRUD de Clientes)
Componente de administración para gestionar la creación, modificación y eliminación de Clientes (acceso exclusivo para los roles Admin o Analyst).

```typescript
// src/app/features/clients/clients-list.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ClientsService } from '../../core/services/clients.service';
import { AuthService } from '../../core/services/auth.service';
import { Client } from '../../core/models/client.models';

@Component({
  selector: 'app-clients-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="clients-container">
      <div class="header">
        <h1>Clientes</h1>
        @if (auth.isAdminOrAnalyst()) {
          <button (click)="openCreateModal()" class="btn-primary">+ Nuevo Cliente</button>
        }
      </div>

      @if (clientsResource.isLoading()) {
        <div class="loading">Cargando clientes...</div>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Empresa</th>
              <th>Email</th>
              <th>Teléfono</th>
              <th>Proyectos</th>
              @if (auth.isAdminOrAnalyst()) {
                <th>Acciones</th>
              }
            </tr>
          </thead>
          <tbody>
            @for (client of clientsResource.value()?.items; track client.id) {
              <tr>
                <td>{{ client.name }}</td>
                <td>{{ client.company || 'N/A' }}</td>
                <td>{{ client.email }}</td>
                <td>{{ client.phone || 'N/A' }}</td>
                <td><span class="count-badge">{{ client.projectCount }}</span></td>
                @if (auth.isAdminOrAnalyst()) {
                  <td>
                    <button (click)="openEditModal(client)" class="btn-table">Editar</button>
                    @if (auth.isAdmin()) {
                      <button (click)="deleteClient(client.id)" class="btn-table danger">Eliminar</button>
                    }
                  </td>
                }
              </tr>
            } @empty {
              <tr>
                <td [attr.colspan]="auth.isAdminOrAnalyst() ? 6 : 5" class="empty-row">No hay clientes creados.</td>
              </tr>
            }
          </tbody>
        </table>

        @if (clientsResource.value(); as data) {
          <div class="pagination">
            <button (click)="changePage(-1)" [disabled]="!data.hasPreviousPage" class="btn-page">Anterior</button>
            <span>{{ currentPage() }} / {{ data.totalPages }}</span>
            <button (click)="changePage(1)" [disabled]="!data.hasNextPage" class="btn-page">Siguiente</button>
          </div>
        }
      }

      <!-- Modal Crear/Editar -->
      @if (showModal()) {
        <div class="modal-overlay" (click)="closeModal()">
          <div class="modal-card" (click)="$event.stopPropagation()">
            <h2>{{ editingClient() ? 'Editar' : 'Crear' }} Cliente</h2>
            <form [formGroup]="clientForm" (ngSubmit)="onSubmit()">
              <div class="form-group">
                <label>Nombre *</label>
                <input formControlName="name" placeholder="Ej: John Doe" />
              </div>
              <div class="form-group">
                <label>Email *</label>
                <input formControlName="email" type="email" placeholder="ejemplo@correo.com" />
              </div>
              <div class="form-group">
                <label>Empresa</label>
                <input formControlName="company" placeholder="Ej: Acme Corp" />
              </div>
              <div class="form-group">
                <label>Teléfono</label>
                <input formControlName="phone" placeholder="Ej: +34 123456789" />
              </div>
              <div class="form-group">
                <label>Notas</label>
                <textarea formControlName="notes" rows="2" placeholder="Detalles de contacto..."></textarea>
              </div>
              <div class="modal-actions">
                <button type="button" (click)="closeModal()" class="btn-sec">Cancelar</button>
                <button type="submit" [disabled]="clientForm.invalid" class="btn-primary">Guardar</button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
    .data-table { width: 100%; border-collapse: collapse; background: white; border-radius: 0.5rem; overflow: hidden; border: 1px solid #e2e8f0; }
    .data-table th, .data-table td { padding: 1rem; text-align: left; border-bottom: 1px solid #e2e8f0; }
    .data-table th { background: #f8fafc; color: #475569; font-weight: 600; }
    .count-badge { background: #f1f5f9; padding: 0.25rem 0.5rem; border-radius: 0.25rem; font-weight: 600; }
    .btn-table { padding: 0.35rem 0.7rem; font-size: 0.85rem; border: 1px solid #cbd5e1; background: white; border-radius: 0.25rem; cursor: pointer; margin-right: 0.5rem; }
    .btn-table.danger { background: #fee2e2; color: #b91c1c; border-color: #fecaca; }
    .empty-row { text-align: center; color: #64748b; padding: 2rem !important; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 1.5rem; }
    .btn-page { background: white; border: 1px solid #cbd5e1; padding: 0.5rem 1rem; border-radius: 0.375rem; cursor: pointer; }
    .btn-page:disabled { opacity: 0.5; cursor: not-allowed; }
  `]
})
export class ClientsListComponent {
  private readonly clientsService = inject(ClientsService);
  protected readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly currentPage = signal(1);
  readonly showModal = signal(false);
  readonly editingClient = signal<Client | null>(null);

  readonly clientForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    company: [''],
    phone: [''],
    notes: ['']
  });

  readonly clientsResource = rxResource({
    request: () => ({ page: this.currentPage() }),
    loader: ({ request }) => this.clientsService.getClients(request.page, 20).pipe(
      map(res => res.data)
    )
  });

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
  }

  openCreateModal() {
    this.editingClient.set(null);
    this.clientForm.reset();
    this.showModal.set(true);
  }

  openEditModal(client: Client) {
    this.editingClient.set(client);
    this.clientForm.setValue({
      name: client.name,
      email: client.email,
      company: client.company ?? '',
      phone: client.phone ?? '',
      notes: client.notes ?? ''
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingClient.set(null);
  }

  onSubmit() {
    if (this.clientForm.invalid) return;

    const data = this.clientForm.value;
    const client = this.editingClient();

    const request$ = client
      ? this.clientsService.updateClient(client.id, data as Partial<Client>)
      : this.clientsService.createClient(data as Partial<Client>);

    request$.subscribe(() => {
      this.clientsResource.reload();
      this.closeModal();
    });
  }

  deleteClient(id: string) {
    if (!confirm('¿Estás seguro de eliminar este cliente? Esta acción no se puede deshacer.')) return;
    this.clientsService.deleteClient(id).subscribe(() => this.clientsResource.reload());
  }
}
```

---

### 6.5 `ProjectsListComponent` (CRUD de Proyectos)
Permite gestionar la visualización y CRUD de Proyectos, permitiendo la asignación de Clientes y Analistas en base a listas dinámicas.

```typescript
// src/app/features/projects/projects-list.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map, forkJoin } from 'rxjs';
import { ProjectsService } from '../../core/services/projects.service';
import { ClientsService } from '../../core/services/clients.service';
import { UsersService } from '../../core/services/users.service';
import { AuthService } from '../../core/services/auth.service';
import { Project, ProjectStatus } from '../../core/models/project.models';

@Component({
  selector: 'app-projects-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="projects-container">
      <div class="header">
        <h1>Proyectos</h1>
        @if (auth.isAdminOrAnalyst()) {
          <button (click)="openCreateModal()" class="btn-primary">+ Nuevo Proyecto</button>
        }
      </div>

      @if (projectsResource.isLoading()) {
        <div class="loading">Cargando proyectos...</div>
      } @else {
        <div class="projects-grid">
          @for (project of projectsResource.value()?.items; track project.id) {
            <div class="project-card">
              <div class="project-header">
                <h3>{{ project.title }}</h3>
                <span class="status-badge" [ngClass]="project.status.toLowerCase()">{{ project.status }}</span>
              </div>
              <p class="description">{{ project.description || 'Sin descripción' }}</p>
              
              <div class="meta-info">
                <div><strong>Cliente:</strong> {{ project.clientName }}</div>
                <div><strong>Analista:</strong> {{ project.analystFullName }}</div>
                @if (project.startDate) {
                  <div><strong>Inicio:</strong> {{ project.startDate | date:'dd/MM/yyyy' }}</div>
                }
              </div>

              @if (auth.isAdminOrAnalyst()) {
                <div class="card-footer">
                  <button (click)="openEditModal(project)" class="btn-table">Editar</button>
                  @if (auth.isAdmin()) {
                    <button (click)="deleteProject(project.id)" class="btn-table danger">Eliminar</button>
                  }
                </div>
              }
            </div>
          } @empty {
            <div class="empty-state">No hay proyectos activos.</div>
          }
        </div>

        @if (projectsResource.value(); as data) {
          <div class="pagination">
            <button (click)="changePage(-1)" [disabled]="!data.hasPreviousPage" class="btn-page">Anterior</button>
            <span>Página {{ currentPage() }} de {{ data.totalPages }}</span>
            <button (click)="changePage(1)" [disabled]="!data.hasNextPage" class="btn-page">Siguiente</button>
          </div>
        }
      }

      <!-- Modal de Creación/Edición -->
      @if (showModal()) {
        <div class="modal-overlay" (click)="closeModal()">
          <div class="modal-card" (click)="$event.stopPropagation()">
            <h2>{{ editingProject() ? 'Editar' : 'Crear' }} Proyecto</h2>
            <form [formGroup]="projectForm" (ngSubmit)="onSubmit()">
              <div class="form-group">
                <label>Título *</label>
                <input formControlName="title" />
              </div>
              <div class="form-group">
                <label>Descripción</label>
                <textarea formControlName="description"></textarea>
              </div>
              <div class="form-row">
                <div class="form-group flex-1">
                  <label>Estado *</label>
                  <select formControlName="status">
                    <option *ngFor="let status of statuses" [value]="status">{{ status }}</option>
                  </select>
                </div>
                <div class="form-group flex-1">
                  <label>Cliente *</label>
                  <select formControlName="clientId">
                    <option value="">Seleccione...</option>
                    <option *ngFor="let c of clients()" [value]="c.id">{{ c.name }}</option>
                  </select>
                </div>
              </div>
              <div class="form-group">
                <label>Analista Asignado *</label>
                <select formControlName="analystId">
                  <option value="">Seleccione...</option>
                  <option *ngFor="let u of analysts()" [value]="u.id">{{ u.fullName }}</option>
                </select>
              </div>
              <div class="form-row">
                <div class="form-group flex-1">
                  <label>Fecha Inicio</label>
                  <input formControlName="startDate" type="date" />
                </div>
                <div class="form-group flex-1">
                  <label>Fecha Fin</label>
                  <input formControlName="endDate" type="date" />
                </div>
              </div>
              <div class="modal-actions">
                <button type="button" (click)="closeModal()" class="btn-sec">Cancelar</button>
                <button type="submit" [disabled]="projectForm.invalid" class="btn-primary">Guardar</button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .projects-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
    .project-card { background: white; border-radius: 0.75rem; border: 1px solid #e2e8f0; padding: 1.5rem; display: flex; flex-direction: column; gap: 0.75rem; }
    .project-header { display: flex; justify-content: space-between; align-items: start; }
    .project-header h3 { margin: 0; color: #0f172a; }
    .status-badge { font-size: 0.75rem; padding: 0.25rem 0.5rem; border-radius: 0.25rem; font-weight: 600; text-transform: uppercase; }
    .status-badge.active { background: #d1fae5; color: #065f46; }
    .status-badge.planning { background: #dbeafe; color: #1d4ed8; }
    .status-badge.onhold { background: #fef3c7; color: #b45309; }
    .description { color: #64748b; font-size: 0.875rem; margin: 0; min-height: 40px; }
    .meta-info { font-size: 0.85rem; color: #334155; display: flex; flex-direction: column; gap: 0.25rem; border-top: 1px solid #f1f5f9; padding-top: 0.75rem; }
    .card-footer { display: flex; gap: 0.5rem; justify-content: flex-end; margin-top: auto; padding-top: 1rem; }
    .form-row { display: flex; gap: 1rem; }
    .flex-1 { flex: 1; }
  `]
})
export class ProjectsListComponent {
  private readonly projectsService = inject(ProjectsService);
  private readonly clientsService = inject(ClientsService);
  private readonly usersService = inject(UsersService);
  protected readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly currentPage = signal(1);
  readonly showModal = signal(false);
  readonly editingProject = signal<Project | null>(null);

  readonly clients = signal<any[]>([]);
  readonly analysts = signal<any[]>([]);

  readonly statuses = Object.values(ProjectStatus);

  readonly projectForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    status: [ProjectStatus.Planning, Validators.required],
    clientId: ['', Validators.required],
    analystId: ['', Validators.required],
    startDate: [''],
    endDate: ['']
  });

  readonly projectsResource = rxResource({
    request: () => ({ page: this.currentPage() }),
    loader: ({ request }) => this.projectsService.getProjects(request.page, 20).pipe(
      map(res => res.data)
    )
  });

  constructor() {
    // Carga de catálogos auxiliares para Clientes y Analistas en modo formulario
    forkJoin({
      clients: this.clientsService.getClients(1, 100).pipe(map(r => r.data.items)),
      users: this.usersService.getUsers(1, 100).pipe(map(r => r.data.items))
    }).subscribe(({ clients, users }) => {
      this.clients.set(clients);
      // Filtramos únicamente analistas y administradores para asignar
      this.analysts.set(users.filter(u => u.role === 1 || u.role === 2));
    });
  }

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
  }

  openCreateModal() {
    this.editingProject.set(null);
    this.projectForm.reset({ status: ProjectStatus.Planning });
    this.showModal.set(true);
  }

  openEditModal(project: Project) {
    this.editingProject.set(project);
    this.projectForm.setValue({
      title: project.title,
      description: project.description ?? '',
      status: project.status,
      clientId: project.clientId,
      analystId: project.analystId,
      startDate: project.startDate ? project.startDate.substring(0, 10) : '',
      endDate: project.endDate ? project.endDate.substring(0, 10) : ''
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingProject.set(null);
  }

  onSubmit() {
    if (this.projectForm.invalid) return;

    const data = this.projectForm.value;
    const project = this.editingProject();

    const request$ = project
      ? this.projectsService.updateProject(project.id, data as Partial<Project>)
      : this.projectsService.createProject(data as Partial<Project>);

    request$.subscribe(() => {
      this.projectsResource.reload();
      this.closeModal();
    });
  }

  deleteProject(id: string) {
    if (!confirm('¿Desea eliminar permanentemente este proyecto?')) return;
    this.projectsService.deleteProject(id).subscribe(() => this.projectsResource.reload());
  }
}
```

---

### 6.6 `TagManagerComponent` (Gestión de Etiquetas)
Permite al administrador ver, añadir y eliminar etiquetas globales del sistema. El listado utiliza la persistencia de caché automática.

```typescript
// src/app/features/admin/tags-manager.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { TagsService } from '../../core/services/tags.service';

@Component({
  selector: 'app-tag-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="tags-manager">
      <h3>Gestión de Etiquetas Globales</h3>

      <form [formGroup]="tagForm" (ngSubmit)="onCreateTag()" class="tag-form">
        <input formControlName="name" placeholder="Nueva etiqueta (ej: Bug)" />
        <input formControlName="color" type="color" class="color-picker" />
        <button type="submit" [disabled]="tagForm.invalid" class="btn-primary">Crear</button>
      </form>

      @if (tagsResource.isLoading()) {
        <div class="loading">Cargando etiquetas...</div>
      } @else {
        <div class="tags-grid">
          @for (tag of tagsResource.value(); track tag.id) {
            <div class="tag-row" [style.border-left-color]="tag.color">
              <span class="tag-name">{{ tag.name }}</span>
              <button (click)="onDeleteTag(tag.id)" class="btn-delete">×</button>
            </div>
          } @empty {
            <div class="empty-text">No hay etiquetas creadas en el sistema.</div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .tags-manager { background: white; border-radius: 0.75rem; border: 1px solid #e2e8f0; padding: 1.5rem; margin-top: 2rem; }
    .tag-form { display: flex; gap: 1rem; align-items: center; margin-bottom: 1.5rem; }
    .tag-form input[type="text"] { flex: 1; padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 0.25rem; }
    .color-picker { width: 50px; height: 38px; border: 1px solid #cbd5e1; border-radius: 0.25rem; padding: 0; cursor: pointer; }
    .tags-grid { display: flex; flex-wrap: wrap; gap: 0.75rem; }
    .tag-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.35rem 0.75rem; background: #f8fafc; border-left: 4px solid; border-radius: 0.25rem; border: 1px solid #e2e8f0; }
    .tag-name { font-weight: 500; font-size: 0.875rem; }
    .btn-delete { background: none; border: none; font-size: 1.25rem; color: #ef4444; cursor: pointer; padding: 0; line-height: 1; }
    .btn-delete:hover { color: #b91c1c; }
  `]
})
export class TagManagerComponent {
  private readonly tagsService = inject(TagsService);
  private readonly fb = inject(FormBuilder);

  readonly tagForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    color: ['#6366f1', Validators.required]
  });

  readonly tagsResource = rxResource({
    loader: () => this.tagsService.getTags().pipe(map(res => res.data))
  });

  onCreateTag() {
    if (this.tagForm.invalid) return;

    const { name, color } = this.tagForm.value;
    this.tagsService.createTag(name!, color!).subscribe(() => {
      this.tagsResource.reload();
      this.tagForm.reset({ color: '#6366f1' });
    });
  }

  onDeleteTag(id: string) {
    if (!confirm('¿Seguro que deseas eliminar esta etiqueta? Se desasociará de todas las tareas.')) return;
    this.tagsService.deleteTag(id).subscribe(() => this.tagsResource.reload());
  }
}
```

---

### 6.7 `CommentsComponent` (Comentarios Incrustados)
Componente anidado que maneja de manera autónoma los comentarios correspondientes a una tarea mediante `rxResource`.

```typescript
// src/app/features/boards/comments/comments.component.ts
import { Component, inject, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { TasksService } from '../../../core/services/tasks.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-comments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="comments-widget">
      <div class="widget-header">
        <h3>Comentarios</h3>
        <button (click)="onClose.emit()" class="close-btn">×</button>
      </div>

      <div class="comment-input-area">
        <textarea [(ngModel)]="newComment" placeholder="Añadir un comentario..." rows="2"></textarea>
        <button (click)="onSubmitComment()" [disabled]="!newComment.trim()" class="btn-primary">Enviar</button>
      </div>

      @if (commentsResource.isLoading()) {
        <div class="loading">Cargando comentarios...</div>
      } @else {
        <div class="comments-list">
          @for (c of commentsResource.value()?.items; track c.id) {
            <div class="comment-item">
              <div class="comment-header">
                <strong>{{ c.authorFullName }}</strong>
                <span class="date">{{ c.createdAt | date:'dd/MM HH:mm' }}</span>
              </div>
              <p class="content">{{ c.content }}</p>
              @if (auth.user()?.userId === c.authorId || auth.isAdmin()) {
                <button (click)="onDeleteComment(c.id)" class="delete-btn">Eliminar</button>
              }
            </div>
          } @empty {
            <div class="empty">No hay comentarios en esta tarea.</div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .comments-widget { display: flex; flex-direction: column; height: 100%; }
    .widget-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .widget-header h3 { margin: 0; }
    .close-btn { background: none; border: none; font-size: 1.5rem; cursor: pointer; }
    .comment-input-area { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1.5rem; }
    .comment-input-area textarea { padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 0.375rem; resize: none; }
    .comments-list { display: flex; flex-direction: column; gap: 1rem; overflow-y: auto; flex: 1; }
    .comment-item { border: 1px solid #e2e8f0; border-radius: 0.375rem; padding: 0.75rem; background: #f8fafc; position: relative; }
    .comment-header { display: flex; justify-content: space-between; font-size: 0.8rem; margin-bottom: 0.35rem; }
    .comment-header .date { color: #64748b; }
    .comment-item p { margin: 0; font-size: 0.9rem; color: #334155; }
    .delete-btn { font-size: 0.75rem; border: none; background: none; color: #ef4444; padding: 0; cursor: pointer; margin-top: 0.5rem; }
    .delete-btn:hover { text-decoration: underline; }
  `]
})
export class CommentsComponent {
  private readonly tasksService = inject(TasksService);
  protected readonly auth = inject(AuthService);

  readonly taskId = input.required<string>();
  readonly onClose = output<void>();

  readonly newComment = signal('');

  readonly commentsResource = rxResource({
    request: () => ({ taskId: this.taskId() }),
    loader: ({ request }) => this.tasksService.getComments(request.taskId, 1, 100).pipe(
      map(res => res.data)
    )
  });

  onSubmitComment() {
    const content = this.newComment().trim();
    if (!content) return;

    this.tasksService.addComment(this.taskId(), content).subscribe(() => {
      this.commentsResource.reload();
      this.newComment.set('');
    });
  }

  onDeleteComment(commentId: string) {
    if (!confirm('¿Deseas eliminar tu comentario?')) return;
    this.tasksService.deleteComment(this.taskId(), commentId).subscribe(() => {
      this.commentsResource.reload();
    });
  }
}
```

---

## 7. Manejo de CORS

La API de TaskFlow utiliza cabeceras CORS en base a un listado de orígenes permitidos configurado en el servidor. Dado que las aplicaciones de Angular por defecto inician en el puerto `4200`, debes asegurar que tu origen esté registrado.

En tu backend C#, agrega `"http://localhost:4200"` al archivo `appsettings.json`:
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:4200"
]
```

O en la configuración de Docker/Compose (`compose.yaml`):
```yaml
- AllowedOrigins__0=http://localhost:4200
```

---

## Tips y Buenas Prácticas para Angular v22

1. **Uso Exclusivo de `inject()`:** Evita la inyección por constructor clásica. El uso de `inject(MyService)` reduce el código innecesario y se adapta perfectamente al contexto funcional de interceptores y guards.
2. **Carga Reactiva con `rxResource`:** Esta función (de `@angular/core/rxjs-interop`) reduce la necesidad de subscribirse manualmente. Provee señales automáticas de `value`, `isLoading`, `error` y `reload()`, las cuales actualizan automáticamente la vista cuando cambian sus dependencias.
3. **Flujo de Paginación Inteligente:** Al cambiar la signal `currentPage`, cualquier `rxResource` que dependa de ella reaccionará de manera instantánea, cancelando la petición HTTP anterior en caso de estar en curso y lanzando la nueva.
4. **Validación de Respuestas de API:** La API retorna un wrapper de tipo `ApiResponse<T>`. Asegúrate de mapear `.pipe(map(res => res.data))` en tus recursos para acceder directamente al conjunto de datos que tu plantilla requiere.
