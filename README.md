# RentasCortas — API de Gestión de Alquileres Temporales

API REST para la gestión de propiedades de alquiler a corto plazo. Permite a propietarios publicar inmuebles y gestionar reservas, y a huéspedes buscar propiedades, reservar y verificar su identidad.

---

## 1. Requisitos previos

Solo necesitas **Docker** y **Git**. No es necesario instalar .NET localmente — Docker se encarga de todo.

### Docker

- **Linux / Ubuntu:**
  ```bash
  sudo apt install docker.io && sudo apt install docker-compose-plugin
  ```

- **Mac:** descargar Docker Desktop desde https://www.docker.com/products/docker-desktop

- **Windows:** descargar Docker Desktop desde https://www.docker.com/products/docker-desktop

### Git

- **Linux / Ubuntu:** `sudo apt install git`
- **Mac / Windows:** https://git-scm.com/downloads

---

## 2. Levantar el proyecto

```bash
git clone https://github.com/valepsernadev/pruebanet.git
cd PruebaTecnica
docker compose up --build
```

La primera vez tarda unos minutos porque descarga las imágenes de .NET 10 y PostgreSQL 16. Las siguientes ejecuciones son mucho más rápidas.

Una vez levantado:

| Recurso | URL |
|---|---|
| API | http://localhost:8080 |
| Documentación interactiva (Scalar) | http://localhost:8080/scalar |

### Comandos útiles

```bash
docker compose down          # Detener los contenedores
docker compose down -v       # Detener y borrar todos los datos (BD incluida)
docker compose logs -f       # Ver logs en tiempo real
docker compose logs -f api   # Ver solo logs de la API
```

---

## 3. Variables de entorno opcionales

El proyecto funciona completo sin configuración adicional. Sin embargo, hay dos integraciones opcionales que se habilitan configurando claves en `src/RentasCortas/appsettings.json`:

### Validación KYC con IA

```json
"Anthropic": {
  "ApiKey": "tu-api-key-de-anthropic"
}
```

Permite verificar la identidad de los huéspedes subiendo una foto de su documento. Sin esta clave, el endpoint retorna `503 Service Unavailable` con un mensaje explicativo.

### Envío de emails por SMTP

```json
"Smtp": {
  "Host": "smtp.ejemplo.com",
  "Port": 587,
  "Username": "usuario",
  "Password": "contraseña",
  "FromEmail": "noreply@ejemplo.com",
  "FromName": "RentasCortas"
}
```

Sin esta configuración, los emails se registran en logs sin enviarse. Las notificaciones in-app siguen funcionando normalmente.

---

## 4. Arquitectura

### Vertical Slice Architecture

Cada feature del sistema es autónoma: tiene su propio Controller, Service, interfaz y DTOs dentro de su propia carpeta. No hay capas horizontales que crucen todo el proyecto.

```
Features/
├── Auth/            # Registro y login con JWT
├── Inmuebles/       # CRUD de propiedades con imágenes
├── Reservas/        # Gestión de reservas con reglas de negocio
├── Favoritos/       # Lista de favoritos por huésped
├── KYC/             # Verificación de identidad con IA
├── Notificaciones/  # Consulta de notificaciones del usuario
├── Dashboard/       # Métricas para propietarios
└── Reportes/        # Exportación de datos a Excel
```

**¿Por qué esta arquitectura?** Permite agregar, modificar o eliminar features de forma independiente sin afectar al resto del sistema. Cada módulo se puede entender leyendo solo su carpeta, sin necesidad de rastrear dependencias horizontales.

### Stack tecnológico

| Componente | Tecnología |
|---|---|
| Backend | .NET 10, C# 13 |
| Base de datos | PostgreSQL 16 |
| ORM | Entity Framework Core |
| Autenticación | JWT (Bearer tokens) |
| Contenedores | Docker + Docker Compose |
| Documentación | OpenAPI + Scalar |

---

## 5. Decisiones técnicas destacadas

### Anti double-booking

Antes de confirmar una reserva, el servicio verifica que no exista otra reserva confirmada para el mismo inmueble en las fechas solicitadas. Esto se valida a nivel de lógica de negocio en el Service, no con constraints de base de datos, para poder retornar mensajes de error claros al usuario.

### KYC con IA (verificación de identidad)

El huésped sube una foto de su documento de identidad. La imagen se lee en memoria, se convierte a base64 y se envía a la API de Claude (vision) para extraer nombre, apellidos, número de documento y fecha de nacimiento. La imagen nunca se guarda en disco ni en base de datos — se procesa y se descarta inmediatamente.

### Notificaciones omnicanal

Un contrato único (`INotificationService`) permite enviar notificaciones por dos canales: email (vía MailKit/SMTP) e in-app (registro en base de datos). Los módulos que disparan notificaciones no necesitan saber cómo se implementa cada canal.

Eventos que generan notificaciones:
- Registro de usuario → email de bienvenida
- Confirmación de reserva → email + in-app
- Validación KYC → in-app con el resultado
- 1 hora antes del checkout → email + in-app (background service)

### Soft delete

Los inmuebles y usuarios nunca se eliminan físicamente de la base de datos. Se marca el campo `deleted_at` y un filtro global en EF Core los excluye automáticamente de todas las consultas.

### Autenticación diferida

El catálogo de inmuebles y sus filtros son completamente públicos. Solo se requiere autenticación (JWT) para acciones que lo necesitan: reservar, guardar favoritos, verificar identidad, etc.

### Background service para recordatorios

Un `BackgroundService` revisa cada 5 minutos si hay reservas confirmadas cuyo checkout sea dentro de la próxima hora. Si encuentra alguna, envía un recordatorio por email e in-app al huésped. Cada notificación se marca con un tipo único por reserva para evitar duplicados.

---

## 6. Endpoints principales

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Registro de usuario | No |
| POST | `/api/auth/login` | Login (retorna JWT) | No |
| GET | `/api/inmuebles` | Listar inmuebles (filtros opcionales) | No |
| POST | `/api/inmuebles` | Crear inmueble | Owner |
| POST | `/api/reservas` | Crear reserva | Guest |
| PATCH | `/api/reservas/{id}/confirm` | Confirmar reserva | Owner |
| POST | `/api/favoritos/{inmuebleId}` | Agregar a favoritos | Guest |
| GET | `/api/favoritos` | Listar favoritos | Guest |
| POST | `/api/kyc/validate` | Verificar identidad | Guest |
| GET | `/api/notificaciones` | Listar notificaciones | Ambos |
| GET | `/api/dashboard` | Métricas del propietario | Owner |
| GET | `/api/reportes/excel` | Descargar reporte Excel | Owner |

Para ver todos los endpoints con sus parámetros y esquemas, consulta la documentación interactiva en http://localhost:8080/scalar.
