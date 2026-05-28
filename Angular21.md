# Guía de integración Angular 21 + TaskFlow API

Esta guía cubre cómo consumir la TaskFlow API desde una aplicación Angular 21 usando el nuevo sistema de signals, standalone components, functional guards e interceptors.

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

export interface RefreshTokenRequest {
  refreshToken: string;
}

export enum UserRole {
  Member = 0,
  Admin = 1
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

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[] | null;
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
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) return EMPTY;

    return this.http.post<{ data: AuthResponse }>(`${this.baseUrl}/refresh`, { refreshToken }).pipe(
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

## 4. HTTP Interceptor (agrega Bearer + renueva token)

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
        // Intentar renovar el token
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
}
```

---

## 7. Guard de autenticación

```typescript
// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;

  return router.createUrlTree(['/login']);
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

## 8. Rutas

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

## 9. Componente Login (ejemplo completo)

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

## 10. Componente BoardsList con paginación

```typescript
// src/app/features/boards/boards-list/boards-list.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BoardsService } from '../../../core/services/boards.service';
import { Board, PaginatedList } from '../../../core/models/board.models';

@Component({
  selector: 'app-boards-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <h2>Mis Boards</h2>

    @if (loading()) {
      <p>Cargando...</p>
    } @else {
      <div class="boards-grid">
        @for (board of paginated()?.items; track board.id) {
          <div class="board-card" [routerLink]="['/boards', board.id]">
            <h3>{{ board.title }}</h3>
            <p>{{ board.description }}</p>
            <span>{{ board.taskCount }} tareas</span>
          </div>
        }
      </div>

      <!-- Paginación -->
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

  readonly loading = signal(true);
  readonly paginated = signal<PaginatedList<Board> | null>(null);
  readonly currentPage = signal(1);

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.boardsService.getBoards(this.currentPage(), 20).subscribe({
      next: res => {
        this.paginated.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  nextPage() {
    this.currentPage.update(p => p + 1);
    this.load();
  }

  prevPage() {
    this.currentPage.update(p => p - 1);
    this.load();
  }
}
```

---

## 11. Manejo de CORS

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

## 12. Flujo completo de tokens

```
1. Login → recibe accessToken (15 min) + refreshToken (7 días)
2. Cada request → interceptor agrega Authorization: Bearer <accessToken>
3. Si la API devuelve 401 → interceptor llama a /Auth/refresh automáticamente
4. Si el refresh falla → redirige a /login
5. Logout → la API invalida el refreshToken en DB
```

---

## 13. Estructura recomendada del proyecto Angular

```
src/app/
├── core/
│   ├── guards/           auth.guard.ts, admin.guard.ts
│   ├── interceptors/     auth.interceptor.ts
│   ├── models/           auth.models.ts, board.models.ts, task.models.ts
│   └── services/         auth.service.ts, boards.service.ts, tasks.service.ts
├── features/
│   ├── auth/
│   │   ├── login/        login.component.ts
│   │   └── register/     register.component.ts
│   ├── boards/
│   │   ├── boards-list/  boards-list.component.ts
│   │   └── board-detail/ board-detail.component.ts
│   └── admin/            admin.component.ts
├── shared/
│   └── components/       button, input, modal, ...
├── app.config.ts
├── app.routes.ts
└── app.component.ts
```

---

## Tips

- Usar `toSignal()` de `@angular/core/rxjs-interop` para convertir observables a signals sin subscribe manual.
- El campo `role` en `AuthResponse` es numérico (`0` = Member, `1` = Admin) — comparar con el enum `UserRole`.
- La API devuelve `errors: string[] | null` en las respuestas fallidas — mostrarlo en los formularios.
- Para ambientes con HTTPS en producción, cambiar `apiUrl` en el environment de producción.
