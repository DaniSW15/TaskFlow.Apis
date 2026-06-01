# 🚀 Guía de Despliegue en Render

Esta guía te ayudará a desplegar **TaskFlow Backend API** en Render (plan gratuito).

## 📋 Prerrequisitos

1. Cuenta en [Render.com](https://render.com) (gratis)
2. Tu código en un repositorio de GitHub
3. El frontend desplegado en Netlify: `https://ng-tasksflow.netlify.app`

## 🔧 Paso 1: Preparar el Repositorio

Asegúrate de que estos archivos estén en tu repositorio:

- ✅ `render.yaml` (ya configurado)
- ✅ `Dockerfile` (ya configurado)
- ✅ `.dockerignore` (ya configurado)

## 🌐 Paso 2: Conectar Render con GitHub

1. Ve a [Render Dashboard](https://dashboard.render.com)
2. Click en **"New +"** → **"Blueprint"**
3. Conecta tu repositorio de GitHub
4. Autoriza a Render para acceder a tu repositorio

## 📦 Paso 3: Desplegar desde Blueprint

1. Selecciona el repositorio `TaskFlow` (o como lo hayas nombrado)
2. Render detectará automáticamente el archivo `render.yaml`
3. Click en **"Apply"**
4. Render creará automáticamente:
   - 🗄️ Base de datos PostgreSQL (plan free)
   - 🌐 Web Service API (plan free)

## ⚙️ Paso 4: Variables de Entorno (Automáticas)

El archivo `render.yaml` ya configuró todas las variables necesarias:

- `DATABASE_URL` → Se conecta automáticamente a la BD
- `JwtSettings__SecretKey` → Se genera automáticamente
- `AllowedOrigins__0` → `https://ng-tasksflow.netlify.app`
- `AllowedOrigins__1` → `http://localhost:4200`

## 🔍 Paso 5: Verificar el Despliegue

1. Espera a que termine el build (5-10 minutos en el plan free)
2. Una vez desplegado, Render te dará una URL tipo:
   ```
   https://taskflow-api.onrender.com
   ```
3. Verifica el health check:
   ```
   https://taskflow-api.onrender.com/health
   ```
   Deberías ver:
   ```json
   {
     "status": "Healthy",
     "timestamp": "2026-06-01T..."
   }
   ```

## 🔗 Paso 6: Actualizar el Frontend en Angular

En tu proyecto Angular, actualiza la URL del API:

**Archivo: `src/environments/environment.prod.ts`**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://taskflow-api.onrender.com/api'  // ← Tu URL de Render
};
```

Haz commit y push. Netlify reconstruirá automáticamente con la nueva URL.

## ✅ Paso 7: Probar CORS

1. Abre tu app en Netlify: `https://ng-tasksflow.netlify.app`
2. Intenta hacer login
3. El error de CORS debería estar resuelto ✅

## 📝 Notas Importantes sobre el Plan Free de Render

⚠️ **Limitaciones del plan gratuito:**
- El servicio se "duerme" después de 15 minutos de inactividad
- La primera petición después de dormir tarda ~30-60 segundos (cold start)
- 750 horas/mes gratis (suficiente para 1 servicio)
- Base de datos PostgreSQL expira después de 90 días (debes crear una nueva)

💡 **Solución para cold starts:**
- Usa un servicio como [UptimeRobot](https://uptimerobot.com) (gratis) para hacer ping cada 14 minutos
- Configura: `https://taskflow-api.onrender.com/health` cada 14 minutos

## 🐛 Solución de Problemas

### Error: "Build failed"
- Verifica que el Dockerfile esté en la raíz del repo
- Revisa los logs en Render Dashboard

### Error: "Database connection failed"
- Render tarda ~2-3 minutos en crear la BD
- Espera a que la BD esté "Available" antes de desplegar el API

### Error de CORS persiste
- Verifica que `AllowedOrigins__0` tenga exactamente: `https://ng-tasksflow.netlify.app`
- Sin barra final `/`
- Con `https://` (no http)

## 🎉 ¡Listo!

Tu backend ahora está desplegado y conectado con tu frontend en Netlify.

**URLs finales:**
- Frontend: `https://ng-tasksflow.netlify.app`
- Backend: `https://taskflow-api.onrender.com` (reemplaza con tu URL real)
- Health: `https://taskflow-api.onrender.com/health`
- Swagger: `https://taskflow-api.onrender.com/swagger` (si está habilitado)

---

**¿Necesitas ayuda?** Revisa los logs en:
- Render Dashboard → tu servicio → "Logs" tab
