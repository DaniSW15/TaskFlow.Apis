# Guía Rápida: Leer y Ordenar Datos en Next.js 15

> Guía práctica para consumir APIs y organizar información en Next.js con TypeScript

---

## 📦 Stack Recomendado

- **Next.js 15** con App Router
- **TypeScript** para tipado seguro
- **Axios** para peticiones HTTP
- **TanStack Query (React Query)** para caché y estado del servidor
- **Zustand** para estado global (auth, configuración)

---

## 🚀 Instalación Rápida

```bash
npx create-next-app@latest mi-app --typescript --app
cd mi-app
npm install axios @tanstack/react-query zustand
```

---

## 🔧 Configuración Inicial

### 1. Variables de entorno
```env
NEXT_PUBLIC_API_URL=http://localhost:8080/api
```

### 2. Configurar React Query
Crear proveedor en `src/app/providers.tsx` y envolver la app en `layout.tsx`

### 3. Configurar cliente HTTP (Axios)
- Crear instancia base con `baseURL`
- Agregar interceptor para tokens de autenticación
- Manejar renovación automática de tokens en error 401

---

## 📖 Leer Datos de la API

### Estructura Básica con React Query

```typescript
// Hook personalizado
export function useItems(page = 1) {
  return useQuery({
    queryKey: ['items', page],
    queryFn: () => api.get('/items', { 
      params: { pageNumber: page, pageSize: 20 } 
    }).then(r => r.data.data),
  });
}

// Uso en componente
const { data, isLoading, error } = useItems(1);
```

### Conceptos Clave

- **queryKey**: identifica y cachea la consulta
- **queryFn**: función que retorna la promesa con los datos
- **enabled**: condición para ejecutar la query
- **staleTime**: tiempo antes de considerar datos obsoletos

---

## 📑 Tipos de Paginación

### Paginación Offset (Páginas Numeradas)

**Cuándo usar:** Listados con números de página (ej: productos, usuarios)

**Respuesta del API:**
```typescript
interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

**Navegación:**
- Botones "Anterior/Siguiente"
- Mostrar "Página X de Y"
- Usar `hasNextPage` / `hasPreviousPage` para deshabilitar botones

---

### Paginación Cursor (Scroll Infinito)

**Cuándo usar:** Feeds, redes sociales, listados largos sin necesidad de saltar páginas

**Respuesta del API:**
```typescript
interface CursorPaginatedList<T> {
  items: T[];
  nextCursor: string | null;
  hasNextPage: boolean;
  pageSize: number;
}
```

**Navegación:**
- Cargar más al hacer scroll
- Pasar el `nextCursor` en la siguiente petición
- Continuar hasta que `hasNextPage` sea `false`

---

## 🔄 Ordenar Datos

### Opción 1: Ordenamiento del Backend

```typescript
// Pasar parámetros de ordenamiento al API
useQuery({
  queryKey: ['items', sortBy, sortOrder],
  queryFn: () => api.get('/items', {
    params: {
      sortBy: 'createdAt',    // campo a ordenar
      sortOrder: 'desc',       // asc o desc
      pageSize: 20
    }
  })
});
```

**Ventajas:** Más eficiente, no carga datos innecesarios

---

### Opción 2: Ordenamiento en el Cliente

```typescript
// Después de recibir los datos
const sortedItems = [...(data?.items || [])].sort((a, b) => {
  // Por número descendente
  return b.count - a.count;
  
  // Por string alfabético
  return a.name.localeCompare(b.name);
  
  // Por fecha (más reciente primero)
  return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
});
```

**Ventajas:** Cambios instantáneos sin llamadas al servidor

---

## 🎯 Patrones Comunes

### Filtrado + Ordenamiento + Paginación

```typescript
const [filters, setFilters] = useState({
  search: '',
  status: 'all',
  sortBy: 'createdAt',
  page: 1
});

const { data } = useQuery({
  queryKey: ['items', filters],
  queryFn: () => api.get('/items', { params: filters })
});
```

---

### Invalidar Caché Después de Mutaciones

```typescript
const queryClient = useQueryClient();

const createMutation = useMutation({
  mutationFn: (newItem) => api.post('/items', newItem),
  onSuccess: () => {
    // Re-cargar la lista actualizada
    queryClient.invalidateQueries({ queryKey: ['items'] });
  }
});
```

---

## 🔍 Estados de Carga

```typescript
const { data, isLoading, error, isFetching } = useQuery(...);

// isLoading: primera carga
if (isLoading) return <Spinner />;

// error: algo falló
if (error) return <Error message={error.message} />;

// isFetching: recargando en segundo plano
{isFetching && <LoadingIndicator />}

// data: renderizar contenido
return <List items={data.items} />;
```

---

## 📊 Criterios de Ordenamiento Comunes

| Tipo | Ejemplo | Uso |
|------|---------|-----|
| **Numérico** | `taskCount`, `price`, `rating` | Prioridad, popularidad |
| **Alfabético** | `name`, `title` | Directorios, listas A-Z |
| **Fecha** | `createdAt`, `dueDate` | Cronología, vencimientos |
| **Enum/Status** | `priority`, `status` | Flujos de trabajo |
| **Booleano** | `isCompleted`, `isFeatured` | Destacados primero |

---

## ✅ Mejores Prácticas

### Leer Datos
- ✅ Usar `queryKey` específicas para cachear correctamente
- ✅ Habilitar queries solo cuando tengas datos necesarios (`enabled`)
- ✅ Configurar `staleTime` para evitar peticiones innecesarias
- ✅ Manejar estados vacíos (`data?.items.length === 0`)

### Ordenar Datos
- ✅ Ordenar en backend para datasets grandes
- ✅ Ordenar en cliente para interacciones rápidas (< 1000 items)
- ✅ Usar `[...array]` para crear copia antes de `.sort()` (evitar mutación)
- ✅ Permitir múltiples criterios (ej: por prioridad, luego por fecha)

### Paginación
- ✅ Deshabilitar botones cuando no hay más páginas
- ✅ Mostrar indicador de carga durante transiciones
- ✅ Mantener la página actual en la URL (`useSearchParams`)
- ✅ Resetear a página 1 cuando cambien filtros

---

## 🎨 Ejemplo Visual de Flujo

```
Usuario solicita datos
    ↓
React Query verifica caché
    ↓
¿Datos frescos? → Sí → Renderizar
    ↓ No
Llamada a API (Axios)
    ↓
Datos llegan
    ↓
Ordenar/Filtrar (opcional)
    ↓
Renderizar componente
    ↓
Guardar en caché
```

---

## 🔗 Recursos

- [TanStack Query Docs](https://tanstack.com/query/latest)
- [Next.js App Router](https://nextjs.org/docs/app)
- [Axios Interceptors](https://axios-http.com/docs/interceptors)

---

**Última actualización:** Junio 2026
