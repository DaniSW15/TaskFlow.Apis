# TaskFlow API — Guía React / Next.js 15

> **Stack:** Next.js 15 · App Router · TypeScript · Axios · Zustand · TanStack Query

**Base URL:** `http://localhost:8080/api`

---

## Instalación

```bash
npx create-next-app@latest taskflow-app --typescript --app
cd taskflow-app
npm install axios @tanstack/react-query zustand
```

---

## Configuración

```ts
// src/lib/env.ts
export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8080/api';
```

```env
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:8080/api
```

---

## Tipos principales

```ts
// src/types/index.ts
export type UserRole = 0 | 1 | 2 | 3; // Member | Admin | Analyst | Client

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

export interface Board {
  id: string; title: string; description?: string;
  ownerId: string; taskCount: number; createdAt: string; updatedAt: string;
}

export interface TaskItem {
  id: string; title: string; description?: string;
  status: 'Todo' | 'InProgress' | 'Done' | 'Cancelled';
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  dueDate?: string; boardId: string; assigneeId?: string;
  assigneeName?: string; createdAt: string; updatedAt: string;
}

export interface Comment {
  id: string; content: string; taskItemId: string;
  authorId: string; authorFullName: string; createdAt: string;
}

export interface Tag { id: string; name: string; color: string; }

export interface User {
  id: string; firstName: string; lastName: string; fullName: string;
  email: string; role: UserRole; createdAt: string; updatedAt: string;
}

export interface Client {
  id: string; name: string; email: string; phone?: string;
  company?: string; notes?: string; projectCount: number;
  createdAt: string; updatedAt: string;
}

export interface Project {
  id: string; title: string; description?: string;
  status: 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled';
  startDate?: string; endDate?: string;
  clientId: string; clientName: string;
  analystId: string; analystFullName: string;
  boardCount: number; createdAt: string; updatedAt: string;
}
```

---

## Auth Store (Zustand)

```ts
// src/store/auth.store.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { AuthResponse } from '@/types';

interface AuthStore {
  user: AuthResponse | null;
  setUser: (user: AuthResponse) => void;
  clear: () => void;
  isAdmin: () => boolean;
  isAdminOrAnalyst: () => boolean;
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      user: null,
      setUser: (user) => set({ user }),
      clear: () => set({ user: null }),
      isAdmin: () => get().user?.role === 1,
      isAdminOrAnalyst: () => (get().user?.role ?? -1) <= 2 && get().user?.role !== 0,
    }),
    { name: 'tf_auth' }
  )
);
```

---

## Cliente HTTP (Axios)

```ts
// src/lib/api.ts
import axios from 'axios';
import { API_URL } from './env';
import { useAuthStore } from '@/store/auth.store';

export const api = axios.create({ baseURL: API_URL });

// Adjunta token automáticamente
api.interceptors.request.use((config) => {
  const user = useAuthStore.getState().user;
  if (user?.accessToken) {
    config.headers.Authorization = `Bearer ${user.accessToken}`;
  }
  return config;
});

// Renueva token en 401
api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true;
      try {
        const { user } = useAuthStore.getState();
        const { data } = await axios.post(`${API_URL}/Auth/refresh`, {
          accessToken: user?.accessToken,
          refreshToken: user?.refreshToken,
        });
        useAuthStore.getState().setUser(data.data);
        original.headers.Authorization = `Bearer ${data.data.accessToken}`;
        return api(original);
      } catch {
        useAuthStore.getState().clear();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
```

---

## Proveedor de React Query

```tsx
// src/app/providers.tsx
'use client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';

export function Providers({ children }: { children: React.ReactNode }) {
  const [qc] = useState(() => new QueryClient());
  return <QueryClientProvider client={qc}>{children}</QueryClientProvider>;
}
```

```tsx
// src/app/layout.tsx
import { Providers } from './providers';
export default function RootLayout({ children }: { children: React.ReactNode }) {
  return <html><body><Providers>{children}</Providers></body></html>;
}
```

---

## Hooks de Auth

```ts
// src/hooks/useAuth.ts
import { useMutation } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import { useAuthStore } from '@/store/auth.store';
import { ApiResponse, AuthResponse } from '@/types';

export function useLogin() {
  const { setUser } = useAuthStore();
  const router = useRouter();

  return useMutation({
    mutationFn: (body: { email: string; password: string }) =>
      api.post<ApiResponse<AuthResponse>>('/Auth/login', body).then(r => r.data.data),
    onSuccess: (user) => { setUser(user); router.push('/boards'); },
  });
}

export function useRegister() {
  const { setUser } = useAuthStore();
  const router = useRouter();

  return useMutation({
    mutationFn: (body: { firstName: string; lastName: string; email: string; password: string }) =>
      api.post<ApiResponse<AuthResponse>>('/Auth/register', body).then(r => r.data.data),
    onSuccess: (user) => { setUser(user); router.push('/boards'); },
  });
}

export function useLogout() {
  const { clear } = useAuthStore();
  const router = useRouter();

  return useMutation({
    mutationFn: () => api.post('/Auth/logout'),
    onSettled: () => { clear(); router.push('/login'); },
  });
}
```

---

## Hooks de datos

```ts
// src/hooks/useBoards.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { ApiResponse, Board, PaginatedList } from '@/types';

export const boardKeys = {
  all: ['boards'] as const,
  list: (page: number) => ['boards', page] as const,
};

export function useBoards(page = 1) {
  return useQuery({
    queryKey: boardKeys.list(page),
    queryFn: () =>
      api.get<ApiResponse<PaginatedList<Board>>>('/Boards', { params: { pageNumber: page, pageSize: 20 } })
         .then(r => r.data.data),
  });
}

export function useCreateBoard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: { title: string; description?: string }) =>
      api.post<ApiResponse<string>>('/Boards', body),
    onSuccess: () => qc.invalidateQueries({ queryKey: boardKeys.all }),
  });
}

export function useDeleteBoard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/Boards/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: boardKeys.all }),
  });
}
```

```ts
// src/hooks/useTasks.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { ApiResponse, TaskItem, PaginatedList, CursorPaginatedList, Comment } from '@/types';

// Paginación offset
export function useTasks(boardId: string, page = 1) {
  return useQuery({
    queryKey: ['tasks', boardId, page],
    queryFn: () =>
      api.get<ApiResponse<PaginatedList<TaskItem>>>('/Tasks', {
        params: { boardId, pageNumber: page, pageSize: 50 }
      }).then(r => r.data.data),
    enabled: !!boardId,
  });
}

// Paginación cursor (infinite scroll)
export function useTasksCursor(boardId: string, cursor?: string) {
  return useQuery({
    queryKey: ['tasks-cursor', boardId, cursor],
    queryFn: () =>
      api.get<ApiResponse<CursorPaginatedList<TaskItem>>>('/Tasks/cursor', {
        params: { boardId, pageSize: 20, ...(cursor && { cursor }) }
      }).then(r => r.data.data),
    enabled: !!boardId,
  });
}

// Comments
export function useComments(taskId: string) {
  return useQuery({
    queryKey: ['comments', taskId],
    queryFn: () =>
      api.get<ApiResponse<PaginatedList<Comment>>>(`/Tasks/${taskId}/comments`)
         .then(r => r.data.data),
  });
}

export function useAddComment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (content: string) =>
      api.post(`/Tasks/${taskId}/comments`, { content }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['comments', taskId] }),
  });
}
```

```ts
// src/hooks/useTags.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { ApiResponse, Tag } from '@/types';

export function useTags() {
  return useQuery({
    queryKey: ['tags'],
    queryFn: () => api.get<ApiResponse<Tag[]>>('/Tags').then(r => r.data.data),
    staleTime: 10 * 60 * 1000, // 10 min — igual que el caché del servidor
  });
}

export function useAddTagToTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, tagId }: { taskId: string; tagId: string }) =>
      api.post(`/Tasks/${taskId}/tags/${tagId}`),
    onSuccess: (_, { taskId }) => qc.invalidateQueries({ queryKey: ['tasks', taskId] }),
  });
}
```

---

## Ejemplo: página de Login

```tsx
// src/app/login/page.tsx
'use client';
import { useState } from 'react';
import { useLogin } from '@/hooks/useAuth';

export default function LoginPage() {
  const login = useLogin();
  const [form, setForm] = useState({ email: '', password: '' });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login.mutate(form);
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Iniciar sesión</h2>
      <input
        type="email" placeholder="Email" value={form.email}
        onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
      />
      <input
        type="password" placeholder="Contraseña" value={form.password}
        onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
      />
      {login.isError && (
        <p className="error">
          {(login.error as any)?.response?.data?.message ?? 'Error al iniciar sesión'}
        </p>
      )}
      <button type="submit" disabled={login.isPending}>
        {login.isPending ? 'Cargando...' : 'Entrar'}
      </button>
    </form>
  );
}
```

---

## Ejemplo: lista de Boards

```tsx
// src/app/boards/page.tsx
'use client';
import { useState } from 'react';
import { useBoards, useCreateBoard, useDeleteBoard } from '@/hooks/useBoards';

export default function BoardsPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useBoards(page);
  const create = useCreateBoard();
  const remove = useDeleteBoard();

  if (isLoading) return <p>Cargando...</p>;

  return (
    <div>
      <button onClick={() => create.mutate({ title: 'Nuevo Board' })}>
        + Crear Board
      </button>

      {data?.items.map(board => (
        <div key={board.id}>
          <h3>{board.title}</h3>
          <p>{board.taskCount} tareas</p>
          <button onClick={() => remove.mutate(board.id)}>Eliminar</button>
        </div>
      ))}

      <div>
        <button onClick={() => setPage(p => p - 1)} disabled={!data?.hasPreviousPage}>Anterior</button>
        <span>{page} / {data?.totalPages}</span>
        <button onClick={() => setPage(p => p + 1)} disabled={!data?.hasNextPage}>Siguiente</button>
      </div>
    </div>
  );
}
```

---

## Middleware de protección de rutas

```ts
// src/middleware.ts
import { NextRequest, NextResponse } from 'next/server';

export function middleware(req: NextRequest) {
  // Zustand persiste en localStorage, no en cookies → verificar con cookie propia
  // Alternativa simple: redirigir desde el componente si !user
  return NextResponse.next();
}
```

> Para proteger rutas del lado servidor, leer la cookie del token o usar un HOC:

```tsx
// src/components/ProtectedRoute.tsx
'use client';
import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/auth.store';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user } = useAuthStore();
  const router = useRouter();

  useEffect(() => {
    if (!user) router.push('/login');
  }, [user, router]);

  if (!user) return null;
  return <>{children}</>;
}
```

---

## Estructura del proyecto

```
src/
├── app/
│   ├── layout.tsx          providers (QueryClient)
│   ├── login/page.tsx
│   ├── register/page.tsx
│   ├── boards/
│   │   ├── page.tsx        lista de boards
│   │   └── [id]/page.tsx   detalle + tareas
│   └── admin/page.tsx
├── components/
│   └── ProtectedRoute.tsx
├── hooks/
│   ├── useAuth.ts
│   ├── useBoards.ts
│   ├── useTasks.ts
│   └── useTags.ts
├── lib/
│   ├── api.ts              instancia Axios + interceptores
│   └── env.ts
├── store/
│   └── auth.store.ts       Zustand
└── types/
    └── index.ts
```

---

## Tips rápidos

- `useQuery` cachea automáticamente — no repite la llamada si los datos son frescos.
- `invalidateQueries` tras mutación → refresca la lista sin `window.location.reload()`.
- `staleTime: 10 * 60 * 1000` en `useTags` → respeta el caché del servidor (10 min).
- Para **infinite scroll** de tareas: usar `useInfiniteQuery` con `nextCursor` como `pageParam`.
- `role === 1` = Admin, `role === 2` = Analyst — condicionar botones con `useAuthStore().user?.role`.
- Los errores de la API vienen en `error.response.data.errors[]` (array) o `.message` (string).
