docs/architecture.md — Arquitectura del Proyecto

## 1. Estilo arquitectónico

Vertical Slice Architecture

Cada feature es autónoma: su Controller, su Service, sus DTOs, todo junto en una carpeta.
No hay capas horizontales que crucen todo el proyecto.
Si una feature no existe, el resto no se ve afectado.

Regla de oro: si un archivo solo lo usa una feature, vive dentro de esa feature.
Si dos o más features lo necesitan, va en Common/.


## 2. Estructura de carpetas

src/
└── RentasCortas/
├── Features/
│   ├── Auth/
│   │   ├── AuthController.cs
│   │   ├── IAuthService.cs
│   │   ├── AuthService.cs
│   │   └── AuthDTO.cs
│   ├── Inmuebles/
│   │   ├── InmueblesController.cs
│   │   ├── IInmueblesService.cs
│   │   ├── InmueblesService.cs
│   │   └── InmueblesDTO.cs
│   ├── Dashboard/
│   │   ├── DashboardController.cs
│   │   ├── IDashboardService.cs
│   │   ├── DashboardService.cs
│   │   └── DashboardDTO.cs
│   ├── Reservas/
│   │   ├── ReservasController.cs
│   │   ├── IReservasService.cs
│   │   ├── ReservasService.cs
│   │   └── ReservasDTO.cs
│   ├── Favoritos/
│   │   ├── FavoritosController.cs
│   │   ├── IFavoritosService.cs
│   │   ├── FavoritosService.cs
│   │   └── FavoritosDTO.cs
│   ├── KYC/
│   │   ├── KYCController.cs
│   │   ├── IKYCService.cs
│   │   ├── KYCService.cs
│   │   └── KYCDTO.cs
│   ├── Notificaciones/
│   │   ├── NotificacionesController.cs
│   │   ├── INotificacionesService.cs
│   │   ├── NotificacionesService.cs
│   │   └── NotificacionesDTO.cs
│   └── Reportes/
│       ├── ReportesController.cs
│       ├── IReportesService.cs
│       ├── ReportesService.cs
│       └── ReportesDTO.cs
├── Common/
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs
│   ├── Security/
│   │   └── JwtHelper.cs
│   ├── Storage/
│   │   └── FileStorageService.cs
│   └── Notifications/
│       ├── INotificationService.cs
│       ├── EmailNotificationService.cs
│       └── InAppNotificationService.cs
├── Models/
│   ├── Usuario.cs
│   ├── Inmueble.cs
│   ├── Reserva.cs
│   ├── ReservationStatusHistory.cs
│   ├── Favorito.cs
│   ├── KYCValidation.cs
│   ├── Notificacion.cs
│   ├── PropertyImage.cs
│   └── Review.cs
├── Program.cs
└── appsettings.json


## 3. Flujo de una petición

Request HTTP
→ Controller        (recibe DTO, valida formato, llama al Service)
→ Service           (lógica de negocio, acceso a BD)
→ PostgreSQL
→ Service           (mapea resultado a DTO de respuesta)
→ Controller        (retorna código HTTP correcto)

El Controller nunca contiene lógica de negocio.
El Service nunca retorna un Model directamente — siempre mapea a DTO.


## 4. Modelos de base de datos

usuarios

sqlCREATE TABLE usuarios (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
email VARCHAR(255) UNIQUE NOT NULL,
password_hash VARCHAR(255) NOT NULL,
full_name VARCHAR(255) NOT NULL,
phone VARCHAR(20),
role VARCHAR(10) NOT NULL CHECK (role IN ('guest', 'owner')),
kyc_status VARCHAR(20) DEFAULT 'pending'
CHECK (kyc_status IN ('pending', 'approved', 'rejected')),
created_at TIMESTAMP DEFAULT NOW(),
deleted_at TIMESTAMP DEFAULT NULL
);

inmuebles

sqlCREATE TABLE inmuebles (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
owner_id UUID NOT NULL REFERENCES usuarios(id),
title VARCHAR(255) NOT NULL,
description TEXT,
location VARCHAR(255) NOT NULL,
price_per_night DECIMAL(10,2) NOT NULL,
status VARCHAR(20) DEFAULT 'active'
CHECK (status IN ('active', 'inactive', 'deleted')),
created_at TIMESTAMP DEFAULT NOW(),
deleted_at TIMESTAMP DEFAULT NULL
);

property_images

sqlCREATE TABLE property_images (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
inmueble_id UUID NOT NULL REFERENCES inmuebles(id),
image_url VARCHAR(500) NOT NULL,
created_at TIMESTAMP DEFAULT NOW()
);

reservas

sqlCREATE TABLE reservas (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
inmueble_id UUID NOT NULL REFERENCES inmuebles(id),
guest_id UUID NOT NULL REFERENCES usuarios(id),
check_in DATE NOT NULL,
check_out DATE NOT NULL,
check_in_time TIME DEFAULT '14:00:00',
check_out_time TIME DEFAULT '12:00:00',
total_price DECIMAL(10,2) NOT NULL,
status VARCHAR(20) DEFAULT 'pending'
CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed')),
created_at TIMESTAMP DEFAULT NOW()
);

reservation_status_history

sqlCREATE TABLE reservation_status_history (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
reserva_id UUID NOT NULL REFERENCES reservas(id),
previous_status VARCHAR(20),
new_status VARCHAR(20) NOT NULL,
changed_at TIMESTAMP DEFAULT NOW()
);

favoritos

sqlCREATE TABLE favoritos (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
guest_id UUID NOT NULL REFERENCES usuarios(id),
inmueble_id UUID NOT NULL REFERENCES inmuebles(id),
created_at TIMESTAMP DEFAULT NOW(),
UNIQUE(guest_id, inmueble_id)
);

kyc_validations

sqlCREATE TABLE kyc_validations (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
user_id UUID NOT NULL REFERENCES usuarios(id),
extracted_name VARCHAR(255),
extracted_lastname VARCHAR(255),
extracted_document_number VARCHAR(50),
extracted_birthdate DATE,
verdict VARCHAR(20) CHECK (verdict IN ('approved', 'rejected')),
processed_at TIMESTAMP,
created_at TIMESTAMP DEFAULT NOW()
);

notificaciones

sqlCREATE TABLE notificaciones (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
user_id UUID NOT NULL REFERENCES usuarios(id),
type VARCHAR(50) NOT NULL,
message TEXT NOT NULL,
channel VARCHAR(20) NOT NULL CHECK (channel IN ('email', 'in_app')),
sent_at TIMESTAMP DEFAULT NOW(),
created_at TIMESTAMP DEFAULT NOW()
);

reviews

sqlCREATE TABLE reviews (
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
inmueble_id UUID NOT NULL REFERENCES inmuebles(id),
guest_id UUID NOT NULL REFERENCES usuarios(id),
rating INTEGER CHECK (rating BETWEEN 1 AND 5),
comment TEXT,
created_at TIMESTAMP DEFAULT NOW()
);


## 5. Notificaciones — dos canales, un contrato

Common/Notifications/
├── INotificationService.cs       ← contrato único que llaman las features
├── EmailNotificationService.cs   ← implementación SMTP
└── InAppNotificationService.cs   ← implementación tabla BD

Eventos que disparan notificación:


Registro de usuario → email
Confirmación de reserva → email + in_app
Validación KYC → in_app
1 hora antes del check_out → email + in_app



## 6. KYC — flujo de validación

Guest sube imagen
→ KYCController recibe el archivo
→ KYCService llama a Claude API (vision) para extraer datos
→ KYCService guarda resultado en kyc_validations
→ KYCService actualiza kyc_status en usuarios
→ Imagen se elimina del servidor inmediatamente
→ KYCService dispara notificación in_app con veredicto

El documento de identidad nunca se persiste en base de datos.
Se procesa en memoria, se extrae la información, y se elimina.


## 7. Reglas de negocio críticas


Double-booking: antes de confirmar una reserva, verificar que no existan reservas en estado confirmed para el mismo inmueble en el rango de fechas solicitado
Check-in / Check-out fijos: toda reserva confirmada tiene check_in_time = 14:00 y check_out_time = 12:00 — estos valores los setea el Service, nunca el cliente
KYC obligatorio: un guest no puede confirmar su primera reserva sin tener kyc_status = 'approved'
Soft delete: inmuebles y usuarios nunca se eliminan físicamente — se setea deleted_at
Autenticación diferida: el catálogo y filtros son públicos — solo se requiere JWT para reservar, guardar favoritos o confirmar pago



## 8. Agregar una nueva feature


Crear carpeta Features/NuevaFeature/
Crear NuevaFeatureController.cs, INuevaFeatureService.cs, NuevaFeatureService.cs, NuevaFeatureDTO.cs
Si el modelo de datos es nuevo, agregarlo a Models/ y al init.sql
Registrar el Service en Program.cs
No tocar ninguna otra feature existente