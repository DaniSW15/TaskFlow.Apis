# 🏗️ Estructura Backend Profesional

```txt
backend/
│
├── src/
│   │
│   ├── TaskFlow.Api/
│   ├── TaskFlow.Application/
│   ├── TaskFlow.Domain/
│   ├── TaskFlow.Infrastructure/
│   └── TaskFlow.Shared/
│
├── tests/
│
├── TaskFlow.sln
├── .gitignore
└── README.md
```

---

# 📁 ¿Qué significa cada carpeta?

## src/

Contiene TODO el código principal del backend.

---

## TaskFlow.Api/

Responsable de:

* Controllers
* JWT
* Swagger
* Middlewares
* configuración HTTP

Ejemplo:

```txt
TaskFlow.Api/
│
├── Controllers/
├── Middleware/
├── Extensions/
├── Program.cs
└── appsettings.json
```

---

## TaskFlow.Application/

Contiene:

* lógica de negocio
* commands
* queries
* DTOs
* validaciones

Ejemplo:

```txt
TaskFlow.Application/
│
├── Features/
├── DTOs/
├── Interfaces/
└── Behaviors/
```

---

## TaskFlow.Domain/

Núcleo principal del sistema.

Contiene:

* Entities
* Enums
* Interfaces
* reglas del negocio

Ejemplo:

```txt
TaskFlow.Domain/
│
├── Entities/
├── Enums/
├── Common/
└── Interfaces/
```

---

## TaskFlow.Infrastructure/

Conecta:

* PostgreSQL
* Entity Framework Core
* servicios externos
* repositories

Ejemplo:

```txt
TaskFlow.Infrastructure/
│
├── Persistence/
├── Repositories/
├── Services/
└── Migrations/
```

---

## TaskFlow.Shared/

Código compartido.

Ejemplo:

* constantes
* helpers
* responses
* exceptions

---

# 🧪 tests/

Pruebas unitarias y testing.

Ejemplo:

```txt
tests/
│
├── TaskFlow.UnitTests/
└── TaskFlow.IntegrationTests/
```

---

# 🚀 Cómo crear estructura

## 1. Crear carpeta backend

```bash
mkdir backend
cd backend
```

---

# 2. Crear solución

```bash
dotnet new sln -n TaskFlow
```

---

# 3. Crear carpeta src

```bash
mkdir src
cd src
```

---

# 4. Crear proyectos

```bash
dotnet new webapi -n TaskFlow.Api

dotnet new classlib -n TaskFlow.Application

dotnet new classlib -n TaskFlow.Domain

dotnet new classlib -n TaskFlow.Infrastructure

dotnet new classlib -n TaskFlow.Shared
```

---

# 5. Regresar a raíz

```bash
cd ..
```

---

# 6. Agregar proyectos a solución

```bash
dotnet sln add src/**/*.csproj
```

---

# 7. Crear tests

```bash
mkdir tests
cd tests

dotnet new xunit -n TaskFlow.UnitTests
```

---

# 8. Agregar tests a solución

```bash
cd ..

dotnet sln add tests/**/*.csproj
```

---

# 📦 Inicializar Git

```bash
git init
```

---

# 📄 Crear .gitignore

```gitignore
bin/
obj/
.vs/
.idea/
.vscode/
.DS_Store
```

---

# 🚀 Primer Commit

```bash
git add .
git commit -m "Initial backend clean architecture setup"
```

---

# ☁️ Subir a GitHub

## Crear repositorio en GitHub

Nombre recomendado:

```txt
taskflow-backend
```

---

# Conectar repositorio

```bash
git remote add origin https://github.com/usuario/taskflow-backend.git
```

---

# Push

```bash
git branch -M main
git push -u origin main
```

---

# ✅ Resultado Profesional

Tu backend quedará:

* limpio
* modular
* enterprise-ready
* escalable
* preparado para frontend futuro

Después puedes crear otro repositorio:

```txt
taskflow-frontend
```

o unirlos más adelante en monorepo.
