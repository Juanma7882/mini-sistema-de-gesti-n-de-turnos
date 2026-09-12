# Mini sistema de gestión de turnos — Prueba técnica MAE Software

Aplicación para gestionar los turnos de una clínica: pacientes, profesionales y
turnos, con dos tipos de usuario (Administrador y Profesional).

> **Estado:** desplegado y funcionando — backend en Railway
> ([`/health`](https://mini-sistema-de-gesti-n-de-turnos-production.up.railway.app/health))
> y frontend en [Vercel](https://gestion-turnos-rho.vercel.app/). Backend
> completo (capas, auth, endpoints, tests de integración, Dockerfile y CI); el
> frontend cubre el flujo funcional completo de las tres entidades — el
> detalle de qué falta pulir vive en [`docs/frontend.md`](docs/frontend.md).

---

## Entrega

[↑ volver al índice](#índice)

| Qué                       | Dónde                                                                                |
| ------------------------- | ------------------------------------------------------------------------------------- |
| **Sistema desplegado**    | Frontend: [gestion-turnos-rho.vercel.app](https://gestion-turnos-rho.vercel.app/) · Backend: [Railway](https://mini-sistema-de-gesti-n-de-turnos-production.up.railway.app) ([`/health`](https://mini-sistema-de-gesti-n-de-turnos-production.up.railway.app/health)) |
| **Repositorio**           | Este mismo repo — código fuente, historial de commits, este README                    |
| **Diagrama de arquitectura** | [gitdiagram.com/Juanma7882/mini-sistema-de-gesti-n-de-turnos](https://gitdiagram.com/Juanma7882/mini-sistema-de-gesti-n-de-turnos) |
| **Video demostrativo**    | [Ver en Google Drive](https://drive.google.com/file/d/1-rRDr_eMJ2oms3cp8eXwu-2h_WIli41X/view?usp=sharing) |
| **Credenciales de prueba** | Ver [Credenciales de prueba](#credenciales-de-prueba)                                |
| **Tecnologías utilizadas** | Ver [Stack](#stack)                                                                   |
| **Decisiones técnicas**   | Ver [Decisiones técnicas (resumen)](#decisiones-técnicas-resumen) — detalle en [`docs/decisiones-tecnicas.md`](docs/decisiones-tecnicas.md) |
| **Uso de IA**             | Ver [Uso de inteligencia artificial (resumen)](#uso-de-inteligencia-artificial-resumen) — detalle en [`docs/uso-de-ia.md`](docs/uso-de-ia.md) |
| **Mejoras futuras**       | Ver [Mejoras futuras (resumen)](#mejoras-futuras-resumen) — detalle en [`docs/mejoras-futuras.md`](docs/mejoras-futuras.md) |

---

## Índice

[Entrega](#entrega) · [Stack](#stack) · [Cómo correr localmente](#cómo-correr-el-proyecto-localmente) ·
[Variables de entorno](#variables-de-entorno) · [Credenciales de prueba](#credenciales-de-prueba) ·
[Decisiones técnicas](#decisiones-técnicas-resumen) · [Uso de IA](#uso-de-inteligencia-artificial-resumen) ·
[Mejoras futuras](#mejoras-futuras-resumen) · [Modelo de datos](#modelo-de-datos) ·
[API](#api) · [Auth y permisos](#autenticación-y-permisos) ·
[Tiempo real](#tiempo-real-signalr) · [Máquina de estados](#máquina-de-estados-del-turno) ·
[Flujos de uso](#flujos-de-uso) · [Validaciones](#validaciones-principales) ·
[Estructura](#estructura-del-proyecto) · [Más documentación](#más-documentación)

---

## Stack

[↑ volver al índice](#índice)

| Capa          | Tecnología                                                                                              |
| ------------- | ------------------------------------------------------------------------------------------------------- |
| Backend       | ASP.NET Core Web API, C# / .NET 9 (capas Domain / Application / Infrastructure / Api)                   |
| Frontend      | React + TypeScript (Vite), Tailwind CSS v4 (componentes propios, sin shadcn/ui) · lucide-react · sonner |
| Base de datos | SQLite + EF Core (migraciones)                                                                          |
| Auth          | JWT propio (access token) + refresh token con rotación · hash BCrypt · rate limit en login              |
| Tiempo real   | SignalR (`/hubs/turnos`) — push de cambios de turno a Admin y al profesional dueño                      |
| Tests         | xUnit + FluentAssertions                                                                                |
| Deploy        | Backend en Railway (Dockerfile + volume para el `.db`) · Frontend en Vercel                             |
| CI            | GitHub Actions (build + test)                                                                           |

---

## Cómo correr el proyecto localmente

[↑ volver al índice](#índice)

```bash
# Backend
cd backend/src/Api
dotnet user-secrets set "Jwt:Secret" "una-clave-de-desarrollo-de-32-caracteres-o-mas"
dotnet restore
dotnet run                     # migra + siembra turnos.db al arrancar — http://localhost:5125

# Frontend
cd frontend
pnpm install
pnpm dev                       # http://localhost:5173
```

No hace falta `dotnet ef database update` a mano: `Program.cs` corre las
migraciones pendientes y siembra los datos de demo (si la base está vacía) en
cada arranque, en cualquier entorno.

`Jwt:Secret` es la única variable que hay que setear para levantar el backend
local (el resto tiene defaults razonables en `appsettings.json`). `dotnet user-
secrets` la guarda fuera del repo; alternativas equivalentes son
`appsettings.Development.json` (ignorado por git) o la variable de entorno
`Jwt__Secret`. Sin ella, la Api levanta pero el primer request que pase por el
middleware de auth (`/api/auth/login` incluido) revienta al construir la
clave HMAC.

Swagger queda disponible en `http://localhost:5125/swagger` (solo en
`Development`, que es el entorno por default de `dotnet run`).

---

## Variables de entorno

[↑ volver al índice](#índice)

### Backend (`backend/src/Api`)

| Variable                     | Descripción                                           | Ejemplo                         |
| ---------------------------- | ----------------------------------------------------- | ------------------------------- |
| `ConnectionStrings__Default` | Ruta del archivo SQLite (en Railway apunta al volume) | `Data Source=./data/turnos.db`  |
| `Jwt__Secret`                | Clave HMAC para firmar el access token                | _(secreto, mín. 32 chars)_      |
| `Jwt__Issuer`                | Emisor del token                                      | `mae-turnos`                    |
| `Jwt__Audience`              | Audiencia del token                                   | `mae-turnos-web`                |
| `Jwt__AccessMinutes`         | Vida del access token                                 | `15`                            |
| `Jwt__RefreshDays`           | Vida del refresh token                                | `14`                            |
| `Cors__AllowedOrigins`       | Orígenes permitidos (URL del frontend)                | `https://gestion-turnos-rho.vercel.app` |
| `Seed__AdminPassword`        | Password del usuario admin sembrado                   | _(secreto)_                     |
| `Seed__ProfessionalPassword` | Password del usuario profesional sembrado             | _(secreto)_                     |

### Frontend (`frontend`)

| Variable             | Descripción                                          | Ejemplo                                 |
| -------------------- | ---------------------------------------------------- | --------------------------------------- |
| `VITE_API_URL`       | URL base de la API                                   | `https://mini-sistema-de-gesti-n-de-turnos-production.up.railway.app/api` |
| `VITE_DEMO_EMAIL`    | Opcional, solo dev: prellena el email del login      | `admin@clinica.test`                    |
| `VITE_DEMO_PASSWORD` | Opcional, solo dev: prellena la contraseña del login | _(secreto local)_                       |

> No se versionan contraseñas ni secretos. Localmente usar `appsettings.Development.json`
> (ignorado por git) o variables de entorno; en producción, las variables de Railway/Vercel.

---

## Credenciales de prueba

[↑ volver al índice](#índice)

Se siembran automáticamente al arrancar si la base está vacía.

| Rol           | Email                       | Password                                                       |
| ------------- | --------------------------- | --------------------------------------------------------------- |
| Administrador | `admin@clinica.test`        | `Admin1234` (o el valor de `Seed__AdminPassword`, si está seteada) |
| Profesional   | `dra.gomez@clinica.test`    | `Profesional1234` (o el valor de `Seed__ProfessionalPassword`)  |
| Profesional   | `dr.fernandez@clinica.test` | `Profesional1234` (o el valor de `Seed__ProfessionalPassword`)  |
| Profesional   | `dra.ruiz@clinica.test`     | `Profesional1234` (o el valor de `Seed__ProfessionalPassword`)  |

`Admin1234` / `Profesional1234` son los defaults que usa el seed
(`DbSeeder`) cuando `Seed__AdminPassword` / `Seed__ProfessionalPassword` no
están seteadas — sirven para correr y probar el proyecto en local sin
configurar nada extra. **En el despliegue público (Railway) esas dos
variables están seteadas a valores propios**, distintos de estos defaults,
justamente porque el fallback queda visible en el código fuente del repo.

Todos los pacientes, profesionales y turnos de ejemplo son **ficticios**.

---

## Decisiones técnicas (resumen)

[↑ volver al índice](#índice)

### Por qué estas tecnologías

- **Backend: ASP.NET Core / .NET 9.** Tipado fuerte, EF Core para migraciones
  y tests de integración contra SQLite real, y Swagger/OpenAPI sin librerías
  extra para que un evaluador pueda probar la API sin Postman.
- **Frontend: React + TypeScript (Vite) + Tailwind v4.** Es una SPA
  autenticada (sin necesidad de SSR ni SEO): Vite da un dev loop rápido y un
  build estático simple de desplegar en Vercel. Tailwind evita mantener una
  librería de componentes de terceros para el volumen de UI de esta prueba.
- **Base de datos: SQLite + EF Core.** Elegida por el tamaño del proyecto:
  al ser una prueba técnica de alcance acotado (una sola clínica), no se
  justifica levantar un servidor de base de datos propio — SQLite vive como
  un archivo dentro del mismo contenedor del backend, lo que evita problemas
  de despliegue (no hay que provisionar, conectar ni mantener un servicio de
  base aparte). Al usar EF Core, migrar a Postgres el día de mañana es
  cambiar el provider y el connection string, no el código de dominio (ver
  [Mejoras futuras](#mejoras-futuras-resumen)).

### Cómo se organizó el proyecto

Backend en capas (Domain → Application → Infrastructure → Api, un
repositorio por agregado) y frontend en anillos (`app` → `features` →
`shared`/`core`) — ver [Estructura del proyecto](#estructura-del-proyecto)
arriba. El razonamiento detallado de cada decisión de layout está en
[`docs/arquitectura-backend.md`](docs/arquitectura-backend.md) y
[`docs/arquitectura-frontend.md`](docs/arquitectura-frontend.md).

### Cómo se implementaron usuarios y permisos

Dos roles (`Admin`/`Profesional`) resueltos por claims firmados en el JWT,
verificados en el servidor en cada request — nunca en el cliente. Detalle
completo en [Autenticación y permisos](#autenticación-y-permisos).

### Cómo se evitó que un profesional tenga dos turnos en el mismo horario

Doble garantía: un pre-chequeo en la capa Application (feedback rápido) más
un índice único parcial en SQLite (`(ProfesionalId, Inicio) WHERE Estado <>
Cancelado`) que corta la carrera real entre dos escrituras concurrentes.
Detalle en [Validaciones principales](#validaciones-principales).

### Validaciones y seguridad básica

FluentValidation por request + `ValidationFilter` global, `ProblemDetails`
(RFC 7807) para todos los errores, contraseñas con BCrypt, refresh token
rotado y hasheado en base, y rate limit en `/auth/login` (5 intentos/60s por
IP) contra fuerza bruta. Detalle completo en
[Validaciones principales](#validaciones-principales) y
[Autenticación y permisos](#autenticación-y-permisos).

> Justificación línea por línea de cada decisión no obvia (por qué JWT
> propio y no Identity, por qué controllers y no minimal APIs, etc.) en
> [`docs/decisiones-tecnicas.md`](docs/decisiones-tecnicas.md).

---

## Uso de inteligencia artificial (resumen)

[↑ volver al índice](#índice)

- **Herramienta:** Claude Code (Sonnet 5), como agente dentro del editor —
  implementación guiada, no autocompletado puntual.
- **Dónde se usó:** todo el proyecto — la IA escribió el código tanto del
  backend como del frontend (capas, auth, endpoints, tests de integración,
  Dockerfile, CI en el backend; scaffolding, auth/guards, CRUD de las tres
  entidades, realtime SignalR y pulido en el frontend).
- **Prompts principales:** planificar, explorar, iterar e implementar, en ese
  orden, repetido en cada bloque de trabajo tanto en frontend como en
  backend. Los distintos `.md` de `docs/` se usaron como *baseline* del
  proyecto — ahí se definía primero qué había que hacer en cada tarea, y
  tenerlo organizado permitió usarlos después como checklist: revisar que
  cada tarea estuviera bien resuelta sin desviarse del plan.
- **Qué se revisó y corrigió:** por cada tarea implementada se revisaba el
  resultado con tests automatizados y probando la funcionalidad a mano,
  buscando bugs activamente (no solo leyendo el código generado). En el
  frontend en particular, varias veces hubo que iterar sobre los estilos
  hasta que la UI funcionara correctamente. Ejemplos concretos en
  [`docs/uso-de-ia.md`](docs/uso-de-ia.md).
- **Qué se decidió personalmente:** todo el diseño (arquitectura, UI,
  decisiones con más de una opción razonable — JWT propio vs. Identity,
  exponer Swagger en la URL pública de Railway, descartar `shadcn/ui`, qué
  quedaba fuera de alcance) fue definido por el desarrollador; la IA apoyaba
  explorando posibles caminos y escribiendo el código de lo que luego se
  decidía implementar, nunca tomando esas decisiones por su cuenta.

> Prompts principales y detalle completo en
> [`docs/uso-de-ia.md`](docs/uso-de-ia.md).

---

## Mejoras futuras (resumen)

[↑ volver al índice](#índice)

**De las funcionalidades pedidas en la consigna, ninguna quedó pendiente** —
todas están implementadas y funcionando. Lo que sigue son mejoras adicionales,
deliberadamente fuera de alcance para este plazo (con el motivo de cada
decisión en el documento completo):

- **Seguridad:** detección de reuso de refresh token, lockout por cuenta
  (hoy solo hay rate limit por IP).
- **Datos:** catálogo con FK para `Especialidad`/`ObraSocial` (hoy texto
  libre normalizado), revertir un turno terminal con auditoría, paginación
  cursor-based.
- **Frontend:** limpiar `shared/components/ui/` (residuo de un `shadcn/ui`
  descartado), una librería de data-fetching (TanStack Query) si crece el
  número de pantallas.
- **Infraestructura:** Postgres administrado en vez de SQLite + volume si
  hubiera que escalar a múltiples instancias, usuario no-root en el
  Dockerfile, CI también para el frontend, health check con liveness/
  readiness separados.

> Detalle completo, con el motivo de cada decisión, en
> [`docs/mejoras-futuras.md`](docs/mejoras-futuras.md).

---

## Modelo de datos

[↑ volver al índice](#índice)

```
Paciente( Id, Nombre, Apellido, Telefono, ObraSocial, CreatedAt, DeletedAt? )
Usuario( Id, Nombre, Apellido, Email[unique], PasswordHash, Rol{Admin|Profesional},
         CreatedAt, DeletedAt? )
Profesional( Id, Especialidad, CreatedAt, UsuarioId[FK 1:1 obligatoria → Usuario] )
Turno( Id, PacienteId, ProfesionalId, Inicio[DateTime], Estado[int], Notas?,
       CreatedAt, UpdatedAt )
RefreshToken( Id, UsuarioId, TokenHash[SHA-256], ExpiresAt, RevokedAt?, ReplacedByHash? )

EstadoTurno: Pendiente=0, Confirmado=1, Cancelado=2, Atendido=3

Regla anti doble-turno:
  UNIQUE (ProfesionalId, Inicio) WHERE Estado <> Cancelado   (índice único parcial)
  + validación en la capa Application (excluyendo el propio Id al editar)
```

Decisiones de modelado:

- **`Usuario` es la única fuente de verdad de la identidad de la persona**
  (`Nombre`/`Apellido`) y de su baja (`DeletedAt`), para cualquier rol —
  Admin incluido, que no tiene `Profesional`. `Profesional` es el perfil de
  negocio (especialidad, turnos) y **siempre** requiere un `Usuario` (FK 1:1
  obligatoria de ese lado); dar de baja al usuario también le corta el login
  al profesional, mismo flag. Detalle y alternativas descartadas en
  [`docs/database.md`](docs/database.md#3-usuario).
- **`Inicio` como un solo `DateTime`** (precisión de minuto). La UI muestra dos
  selectores (fecha + hora); el dominio guarda un instante. Ordenar y consultar
  quedan triviales, y el índice `(ProfesionalId, Inicio)` es literal a "misma
  fecha y horario".
- **Colisión por slot exacto**, no por solape con duración. La base misma
  garantiza la regla mediante el índice único parcial.
- **`Estado` como int enum** en la base; viaja como string en el JSON.
- **Soft delete** (`DeletedAt`) en Paciente y Usuario; el Turno se da de baja
  pasándolo a `Cancelado`. Borrar un paciente/profesional con turnos activos
  responde **409**.
- **Horarios en hora local naïve**: una única clínica, "las 15:00 son las 15:00".

> Esquema completo columna por columna (tipos EF, índices, migraciones) en
> [`docs/database.md`](docs/database.md).

---

## API

[↑ volver al índice](#índice)

Prefijo `/api`. Todas las rutas requieren `Authorization: Bearer <access token>`
excepto `POST /auth/login` y `POST /auth/refresh`.

### Auth

| Método | Ruta            | Rol         | Request               | Éxito                                                 | Errores                                               |
| ------ | --------------- | ----------- | --------------------- | ----------------------------------------------------- | ----------------------------------------------------- |
| POST   | `/auth/login`   | anónimo     | `{ email, password }` | **200** `{ token, user }` + `Set-Cookie: rt`          | 400 · 401 credenciales inválidas · **429** rate limit |
| POST   | `/auth/refresh` | cookie `rt` | —                     | **200** `{ token }` + `Set-Cookie: rt` (rotado)       | 401 cookie ausente/expirada/revocada                  |
| POST   | `/auth/logout`  | Bearer      | —                     | **204** (revoca `rt`)                                 | 401                                                   |
| GET    | `/auth/me`      | Bearer      | —                     | **200** `{ id, nombre, email, role, profesionalId? }` | 401                                                   |

### Pacientes — solo Admin

| Método | Ruta                                 | Request                                      | Éxito                                      | Errores                                        |
| ------ | ------------------------------------ | -------------------------------------------- | ------------------------------------------ | ---------------------------------------------- |
| GET    | `/pacientes?search=&page=&pageSize=` | —                                            | **200** `{ items, total, page, pageSize }` | 401 · 403                                      |
| GET    | `/pacientes/{id}`                    | —                                            | **200** `PacienteDto`                      | 401 · 403 · 404                                |
| POST   | `/pacientes`                         | `{ nombre, apellido, telefono, obraSocial }` | **201** `PacienteDto` + `Location`         | 400 · 401 · 403                                |
| PUT    | `/pacientes/{id}`                    | `{ nombre, apellido, telefono, obraSocial }` | **200** `PacienteDto`                      | 400 · 401 · 403 · 404                          |
| DELETE | `/pacientes/{id}`                    | —                                            | **204** (soft delete)                      | 401 · 403 · 404 · **409** tiene turnos activos |

### Profesionales — escritura solo Admin

| Método | Ruta                                     | Request                                               | Éxito                           | Errores                                        |
| ------ | ---------------------------------------- | ----------------------------------------------------- | ------------------------------- | ---------------------------------------------- |
| GET    | `/profesionales?search=&page=&pageSize=` | —                                                     | **200** `{ items, total, ... }` | 401 · 403                                      |
| GET    | `/profesionales/{id}`                    | —                                                     | **200** `ProfesionalDto`        | 401 · 403 · 404                                |
| POST   | `/profesionales`                         | `{ nombre, apellido, especialidad, email, password }` | **201** `ProfesionalDto`        | 400 · 401 · 403 · **409** email ya registrado  |
| PUT    | `/profesionales/{id}`                    | `{ nombre, apellido, especialidad }`                  | **200** `ProfesionalDto`        | 400 · 401 · 403 · 404                          |
| DELETE | `/profesionales/{id}`                    | —                                                     | **204** (soft delete)           | 401 · 403 · 404 · **409** tiene turnos activos |

> `POST /profesionales` crea el profesional **y** su cuenta de acceso
> (`Rol.Profesional`) en una sola operación — un profesional nunca existe sin
> usuario para loguearse. `email`/`password` no van en `PUT`: editar no
> re-registra credenciales, solo datos (nombre/apellido/especialidad).

### Turnos

| Método | Ruta                                                                       | Rol                                                          | Request                                                            | Éxito                                       | Errores                                                                       |
| ------ | -------------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------ | ------------------------------------------- | ----------------------------------------------------------------------------- |
| GET    | `/turnos?desde=&hasta=&estado=&pacienteId=&profesionalId=&page=&pageSize=` | Admin (todos) · Profesional (forzado a los suyos)            | —                                                                  | **200** `{ items: TurnoDto[], total, ... }` | 401                                                                           |
| GET    | `/turnos/{id}`                                                             | Admin · Profesional (si es suyo)                             | —                                                                  | **200** `TurnoDto`                          | 401 · **404** (no existe **o** no es suyo)                                    |
| POST   | `/turnos`                                                                  | Admin                                                        | `{ pacienteId, profesionalId, inicio, notas? }`                    | **201** `TurnoDto` + `Location`             | 400 · 401 · 403 · 404 paciente/profesional inexistente · **409** slot ocupado |
| PUT    | `/turnos/{id}`                                                             | Admin                                                        | `{ pacienteId, profesionalId, inicio, notas? }` (datos, no estado) | **200** `TurnoDto`                          | 400 · 401 · 403 · 404 · **409** slot ocupado                                  |
| PATCH  | `/turnos/{id}/estado`                                                      | Admin (cualquier transición legal) · Profesional (las suyas) | `{ estado }`                                                       | **200** `TurnoDto`                          | 400 · 401 · **404** (no existe o no es suyo) · **409** transición ilegal      |

> Cancelar un turno = `PATCH /turnos/{id}/estado { estado: "Cancelado" }`.
> No existe `DELETE /turnos`. El `estado` inicial de un turno creado es `Pendiente`.
> `?profesionalId` en `GET /turnos`: para Admin filtra; para Profesional se ignora
> (siempre se aplica el suyo, tomado del claim del JWT).

### DTOs de respuesta

```jsonc
PacienteDto    { id, nombre, apellido, telefono, obraSocial, createdAt }
ProfesionalDto { id, nombre, apellido, especialidad, createdAt }

TurnoDto {
  id,
  inicio,        // "2026-09-15T15:00:00" (hora local naïve)
  estado,        // "Pendiente" | "Confirmado" | "Cancelado" | "Atendido"
  notas,         // string | null
  paciente:    { id, nombre, apellido, telefono, obraSocial },
  profesional: { id, nombre, apellido, especialidad },
  createdAt, updatedAt
}
```

El `TurnoDto` embebe los resúmenes de paciente y profesional para que el listado
se renderice sin llamadas N+1.

### Status codes

| Código  | Cuándo                                                                                          |
| ------- | ----------------------------------------------------------------------------------------------- |
| **200** | GET / PUT / PATCH ok                                                                            |
| **201** | POST que crea (con header `Location`)                                                           |
| **204** | DELETE, logout                                                                                  |
| **400** | Body mal formado, campos faltantes, `inicio` en el pasado, formato inválido                     |
| **401** | Sin token / access token expirado o inválido / credenciales malas / refresh inválido            |
| **403** | Autenticado pero rol incorrecto (Profesional llamando endpoint solo-Admin)                      |
| **404** | Recurso inexistente **o** Profesional accediendo a un turno ajeno                               |
| **409** | Slot ya ocupado · transición de estado ilegal · borrar con turnos activos                       |
| **429** | Más de 5 intentos de `/auth/login` por IP en 60s (rate limit fijo, protege contra fuerza bruta) |
| **500** | Error no controlado                                                                             |

Cuerpo de error: `ProblemDetails` (RFC 7807).

```jsonc
{
  "type": "https://httpstatuses.io/409",
  "title": "Slot no disponible",
  "status": 409,
  "detail": "El profesional ya tiene un turno el 2026-09-15 a las 15:00.",
  "errors": {}, // presente solo en 400 de validación, por campo
}
```

---

## Autenticación y permisos

[↑ volver al índice](#índice)

### Dos tokens

|                | Access token (JWT)                                            | Refresh token                                                 |
| -------------- | ------------------------------------------------------------- | ------------------------------------------------------------- |
| Vida           | ~15 min                                                       | ~14 días                                                      |
| Formato        | JWT HS256 con claims `sub`, `email`, `role`, `profesionalId?` | string opaco aleatorio                                        |
| Dónde vive     | solo en memoria del cliente                                   | cookie `httpOnly` `Secure` `SameSite=None`, `Path=/api/auth`  |
| En el servidor | no se guarda                                                  | se guarda **hasheado** (SHA-256) en `RefreshToken`, revocable |
| Se envía       | `Authorization: Bearer` en cada request                       | automático, solo al endpoint de refresh                       |

Rotación **revoke-on-use**: cada `/auth/refresh` revoca el token usado y emite uno
nuevo. La detección de reuso completa queda como mejora futura.

**Rate limit en `/auth/login`**: máximo 5 intentos por IP cada 60s (ventana fija),
**429** con `ProblemDetails` si se supera. Frena fuerza bruta contra BCrypt; no
reemplaza un lockout por cuenta (ver [`docs/mejoras-futuras.md`](docs/mejoras-futuras.md)).

### Un solo login para ambos roles

`POST /auth/login` es el mismo para Admin y Profesional. La diferencia está en los
claims del JWT emitido:

- **Admin** → `role=Admin`, **sin** claim `profesionalId`.
- **Profesional** → `role=Profesional`, `profesionalId=<su Id>`.

El handler de `GET /turnos` aplica `.Where(t => t.ProfesionalId == claims.profesionalId)`
solo cuando `role=Profesional`. **El cliente nunca envía su `profesionalId`**: se
lee del token firmado por el servidor.

### Dónde se hace cumplir

```
React (Vercel)              ASP.NET Core API (Railway)
─────────────              ─────────────────────────────────────────
oculta botones      │      1. Middleware valida firma + expiración del JWT
según el rol         │      2. [Authorize(Roles="Admin")]  → compuerta por rol → 403
(SOLO UX)            │ ──▶  3. En el handler: si role=Profesional, filtra por
                     │         t.ProfesionalId == claims.profesionalId
 guarda el access    │      4. Acceso por id a un recurso ajeno → 404
 en memoria y lo     │
 manda en cada call  │      Nunca se confía en un profesionalId enviado por el cliente.
```

El gating por rol en el frontend es **solo UX**; la API es la fuente de verdad.

### Hidratación optimista (frontend)

El access token vive solo en memoria, así que un F5 lo borra. Al bootear, la app
no muestra el login de entrada: dispara `POST /auth/refresh` con la cookie; si
responde 200 entra directo, si 401 redirige a `/login`. Un snapshot no sensible
`{ nombre, role }` en `localStorage` sirve solo para pintar el menú correcto al
instante (no es frontera de seguridad).

---

## Máquina de estados del turno

[↑ volver al índice](#índice)

```
                         [Admin: cualquier flecha legal]
                         [Profesional: solo las marcadas *]

        ┌─────────────┐  *Pendiente → Confirmado
        │  Pendiente  │───────────────┐
        └──────┬──────┘               │
               │ *Pendiente → Cancelado
               ▼                      ▼
        ┌─────────────┐        ┌───────────┐
        │ Confirmado  │───────▶│ Cancelado │  (terminal)
        └──────┬──────┘  *Confirmado → Cancelado
               │ *Confirmado → Atendido
               ▼
        ┌─────────────┐
        │  Atendido   │  (terminal)
        └─────────────┘
```

Transiciones fuera del diagrama → **409**. Se valida en la capa Application para
ambos roles; el rol solo define qué transiciones legales puede disparar cada uno.
`Atendido` y `Cancelado` son terminales para todos (revertir = mejora futura con
auditoría).

---

## Tiempo real (SignalR)

[↑ volver al índice](#índice)

Cada cambio de turno (crear, editar, cambiar estado) se empuja por WebSocket a
`/hubs/turnos`, además de quedar disponible por polling vía la API REST normal:

- Cada conexión Admin se suma al grupo `admins`; cada Profesional se suma a
  `profesional-{profesionalId}` (tomado del claim del JWT, igual que en la API REST).
- En cada cambio, el servidor emite el evento `turnoCambiado` al grupo `admins`
  **y** al grupo del profesional dueño — cada cliente recibe solo lo suyo, sin
  filtrar del lado del front.

Detalle de implementación (`TurnoHub`, `ITurnoNotifier`) en
[`docs/arquitectura-backend.md`](docs/arquitectura-backend.md#11-realtime-signalr).

---

## Flujos de uso

[↑ volver al índice](#índice)

### Administrador

1. Login → aterriza en **Turnos** (listado completo). Filtros: rango de fechas,
   estado, profesional, paciente.
2. Carga de datos maestros: crea **profesionales** (nombre, apellido,
   especialidad) y **pacientes** (nombre, apellido, teléfono, obra social).
3. **Crear turno**: elige paciente (buscador), profesional, fecha y hora. Estado
   inicial `Pendiente`. Si el slot está ocupado → 409 con error inline.
4. **Gestionar un turno**: reprogramar (`PUT`, revalida slot), cambiar
   paciente/notas (`PUT`), cambiar estado (`PATCH estado`, cualquier transición
   legal), cancelar (`PATCH estado = Cancelado`, libera el slot).
5. **Mantenimiento de maestros**: editar (`PUT`) y borrar (soft `DELETE`;
   bloqueado con 409 si el paciente/profesional tiene turnos activos).
6. Logout.

### Profesional

1. Login (misma pantalla que el admin) → aterriza en **Mis turnos**, ya filtrado
   por el servidor a sus turnos. Filtros: rango de fechas, estado. El menú no
   muestra Pacientes, Profesionales ni Nuevo turno.
2. **Ver un turno suyo**: datos del paciente (nombre, teléfono, obra social) y del
   turno. Un turno ajeno por URL directa → 404.
3. **Actualizar el estado** de sus turnos (único cambio permitido): confirmar
   (`Pendiente→Confirmado`), marcar atendido (`Confirmado→Atendido`), cancelar
   (`Pendiente/Confirmado→Cancelado`). Transición fuera de eso → 409.
4. **No puede**: crear turnos, reprogramar, cambiar el paciente (el front lo
   oculta; si golpea el endpoint → 403 o 409), ni ver/gestionar pacientes o
   profesionales (403).
5. Logout.

### Transversal — sesión (ambos roles)

- **F5 / reabrir pestaña**: sin flash de login; la app dispara `POST /auth/refresh`
  con la cookie. 200 → entra directo. 401 → `/login`.
- **Access token expira (~15 min)**: el interceptor HTTP detecta el 401, hace
  `POST /auth/refresh` (rota el `rt`) y reintenta el request original. Si el
  refresh falla → `/login`.
- **Logout**: `POST /auth/logout` revoca el `rt` en la tabla, borra la cookie y
  limpia la memoria.

---

## Validaciones principales

[↑ volver al índice](#índice)

FluentValidation por request, corridas por un filtro global (`ValidationFilter`)
que también revisa los errores de _binding_ de ASP.NET Core (JSON malformado,
un enum que no matchea ningún nombre) — los dos caminos terminan en el mismo
`400` con `errors` por campo.

- **Paciente**: `nombre`/`apellido` (obligatorios, ≤80), `telefono` (obligatorio,
  ≤30), `obraSocial` (obligatorio, ≤120).
- **Profesional**: `nombre`/`apellido` (obligatorios, ≤80), `especialidad`
  (obligatorio, ≤120). Alta (`POST`) además: `email` (obligatorio, formato
  válido, ≤256, único — si no, **409**) y `password` (obligatorio, 8-100
  caracteres).
- **Turno**: `pacienteId`/`profesionalId` > 0 en el body; que existan y no
  estén dados de baja se valida en el servicio (necesita la base) → **404**.
  `inicio` obligatorio, a futuro, precisión de minuto (sin segundos).
  `estado` (en `PATCH .../estado`) dentro del enum `EstadoTurno`.
- **Login**: `email` con formato válido, `password` obligatoria.
- **Anti doble-turno**: pre-chequeo en `TurnoService` + índice único parcial en
  SQLite (`(ProfesionalId, Inicio) WHERE Estado <> Cancelado`); la carrera entre
  dos escrituras concurrentes la corta el índice (se captura la
  `DbUpdateException` → **409**).
- **Un paciente no puede tener dos turnos activos con el mismo profesional**:
  mismo patrón — pre-chequeo + segundo índice único parcial
  (`(ProfesionalId, PacienteId) WHERE Estado < Cancelado`).
- **Máquina de estados**: `MaquinaEstados.EsTransicionValida` (Domain, función
  pura) decide qué flechas son legales; el rol solo acota _sobre qué turnos_
  puede dispararlas cada uno, no _qué transiciones_ existen.

---

## Estructura del proyecto

[↑ volver al índice](#índice)

```
├── backend/                    .NET 9 — solución Turnos.sln
│   ├── src/
│   │   ├── Domain/             entidades, enums, reglas de negocio puras
│   │   ├── Application/        casos de uso, validaciones, DTOs, interfaces
│   │   ├── Infrastructure/     EF Core, DbContext, migraciones, repos, JWT, BCrypt
│   │   └── Api/                controllers, pipeline (auth/CORS/errores), DI, seed
│   └── tests/
│       ├── Domain.Tests/       máquina de estados
│       ├── Application.Tests/  servicios contra fakes en memoria
│       ├── Infrastructure.Tests/ índices únicos parciales contra SQLite real
│       └── Api.Tests/          integración: Api real (WebApplicationFactory) + SQLite temporal
├── frontend/                    React + TS (Vite) + Tailwind v4 — anillos app/core/shared/features
│   └── src/
│       ├── app/                 bootstrap: main.tsx, App.tsx, providers, router (todas las rutas)
│       ├── core/                infra transversal: cliente HTTP + interceptor 401, auth (context/guards), layout raíz, realtime (SignalR), env
│       ├── shared/               componentes/hooks/lib reutilizables, sin conocer entidades de dominio
│       ├── features/             auth · pacientes · profesionales · turnos (pages, components, api, hooks, schemas por feature)
│       ├── pages/                404, 403
│       └── styles/               tema Tailwind
├── docs/                       arquitectura, modelo de datos, decisiones, IA, mejoras futuras (ver abajo)
├── scripts/                    smoke.sh — smoke test post-deploy
├── Dockerfile                  build del backend para Railway
├── .dockerignore
└── .github/workflows/          CI (build + test del backend)
```

> Diagrama de arquitectura interactivo (deploy, frontend, backend, base de
> datos y cómo se conectan) generado con [GitDiagram](https://gitdiagram.com):
> [gitdiagram.com/Juanma7882/mini-sistema-de-gesti-n-de-turnos](https://gitdiagram.com/Juanma7882/mini-sistema-de-gesti-n-de-turnos)

---

## Más documentación

[↑ volver al índice](#índice)

Este README es el contrato: qué existe, cómo se usa, y qué decisiones de
modelado son visibles desde afuera (endpoints, DTOs, roles, estados). El
**cómo está armado por dentro** y el **por qué** de cada decisión no obvia
viven en `docs/`:

| Documento                                                                     | Contenido                                                                                   |
| ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| [`docs/arquitectura-backend.md`](docs/arquitectura-backend.md)                | Capas, layout de carpetas, nomenclatura del backend (.NET)                                  |
| [`docs/arquitectura-frontend.md`](docs/arquitectura-frontend.md)              | Anillos `app/features/shared/core`, layout, convenciones del frontend (React)               |
| [`docs/database.md`](docs/database.md)                                        | Esquema completo columna por columna (tipos EF, índices, migraciones)                       |
| [`docs/decisiones-tecnicas.md`](docs/decisiones-tecnicas.md)                  | Por qué se eligió cada cosa no obvia (auth sin Identity, controllers vs minimal APIs, etc.) |
| [`docs/uso-de-ia.md`](docs/uso-de-ia.md)                                      | Herramientas, prompts principales, bugs reales encontrados y corregidos con IA              |
| [`docs/mejoras-futuras.md`](docs/mejoras-futuras.md)                          | Qué quedó deliberadamente fuera de alcance, y por qué                                       |
| [`docs/backend.md`](docs/backend.md) · [`docs/frontend.md`](docs/frontend.md) | Listas de tareas de la implementación (checklists, no contrato)                             |
