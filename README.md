# Mini sistema de gestión de turnos — Prueba técnica MAE Software

Aplicación para gestionar los turnos de una clínica: pacientes, profesionales y
turnos, con dos tipos de usuario (Administrador y Profesional).

> **Estado:** backend completo (capas, auth, endpoints, tests de integración,
> Dockerfile y CI) y desplegable — solo falta cargar la instancia en Railway
> (ver `backend.md` §11.3). El frontend cubre el flujo funcional completo de
> las tres entidades; el detalle de qué falta pulir vive en `frontend.md`.

---

## Stack

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core Web API, C# / .NET 9 (capas Domain / Application / Infrastructure / Api) |
| Frontend | React + TypeScript (Vite), Tailwind CSS + shadcn/ui |
| Base de datos | SQLite + EF Core (migraciones) |
| Auth | JWT propio (access token) + refresh token con rotación · hash BCrypt |
| Tests | xUnit + FluentAssertions |
| Deploy | Backend en Railway (Dockerfile + volume para el `.db`) · Frontend en Vercel |
| CI | GitHub Actions (build + test) |

---

## Cómo correr el proyecto localmente

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

### Backend (`backend/src/Api`)

| Variable | Descripción | Ejemplo |
|---|---|---|
| `ConnectionStrings__Default` | Ruta del archivo SQLite (en Railway apunta al volume) | `Data Source=./data/turnos.db` |
| `Jwt__Secret` | Clave HMAC para firmar el access token | _(secreto, mín. 32 chars)_ |
| `Jwt__Issuer` | Emisor del token | `mae-turnos` |
| `Jwt__Audience` | Audiencia del token | `mae-turnos-web` |
| `Jwt__AccessMinutes` | Vida del access token | `15` |
| `Jwt__RefreshDays` | Vida del refresh token | `14` |
| `Cors__AllowedOrigins` | Orígenes permitidos (URL del frontend) | `https://mae-turnos.vercel.app` |
| `Seed__AdminPassword` | Password del usuario admin sembrado | _(secreto)_ |
| `Seed__ProfessionalPassword` | Password del usuario profesional sembrado | _(secreto)_ |

### Frontend (`frontend`)

| Variable | Descripción | Ejemplo |
|---|---|---|
| `VITE_API_URL` | URL base de la API | `https://mae-turnos.up.railway.app/api` |

> No se versionan contraseñas ni secretos. Localmente usar `appsettings.Development.json`
> (ignorado por git) o variables de entorno; en producción, las variables de Railway/Vercel.

---

## Credenciales de prueba

Se siembran automáticamente al arrancar si la base está vacía.

| Rol | Email | Password |
|---|---|---|
| Administrador | `admin@clinica.test` | valor de `Seed__AdminPassword` |
| Profesional | `dra.gomez@clinica.test` | valor de `Seed__ProfessionalPassword` |
| Profesional | `dr.fernandez@clinica.test` | valor de `Seed__ProfessionalPassword` |
| Profesional | `dra.ruiz@clinica.test` | valor de `Seed__ProfessionalPassword` |

Todos los pacientes, profesionales y turnos de ejemplo son **ficticios**.

---

## Modelo de datos

```
Paciente( Id, Nombre, Apellido, Telefono, ObraSocial, CreatedAt, DeletedAt? )
Profesional( Id, Nombre, Apellido, Especialidad, CreatedAt, DeletedAt? )
Usuario( Id, Email[unique], PasswordHash, Rol{Admin|Profesional}, ProfesionalId? )
Turno( Id, PacienteId, ProfesionalId, Inicio[DateTime], Estado[int], Notas?,
       CreatedAt, UpdatedAt )
RefreshToken( Id, UsuarioId, TokenHash[SHA-256], ExpiresAt, RevokedAt?, ReplacedByHash? )

EstadoTurno: Pendiente=0, Confirmado=1, Cancelado=2, Atendido=3

Regla anti doble-turno:
  UNIQUE (ProfesionalId, Inicio) WHERE Estado <> Cancelado   (índice único parcial)
  + validación en la capa Application (excluyendo el propio Id al editar)
```

Decisiones de modelado:

- **`Inicio` como un solo `DateTime`** (precisión de minuto). La UI muestra dos
  selectores (fecha + hora); el dominio guarda un instante. Ordenar y consultar
  quedan triviales, y el índice `(ProfesionalId, Inicio)` es literal a "misma
  fecha y horario".
- **Colisión por slot exacto**, no por solape con duración. La base misma
  garantiza la regla mediante el índice único parcial.
- **`Estado` como int enum** en la base; viaja como string en el JSON.
- **Soft delete** (`DeletedAt`) en Paciente y Profesional; el Turno se da de baja
  pasándolo a `Cancelado`. Borrar un paciente/profesional con turnos activos
  responde **409**.
- **Horarios en hora local naïve**: una única clínica, "las 15:00 son las 15:00".

---

## API

Prefijo `/api`. Todas las rutas requieren `Authorization: Bearer <access token>`
excepto `POST /auth/login` y `POST /auth/refresh`.

### Auth

| Método | Ruta | Rol | Request | Éxito | Errores |
|---|---|---|---|---|---|
| POST | `/auth/login` | anónimo | `{ email, password }` | **200** `{ token, user }` + `Set-Cookie: rt` | 400 · 401 credenciales inválidas |
| POST | `/auth/refresh` | cookie `rt` | — | **200** `{ token }` + `Set-Cookie: rt` (rotado) | 401 cookie ausente/expirada/revocada |
| POST | `/auth/logout` | Bearer | — | **204** (revoca `rt`) | 401 |
| GET | `/auth/me` | Bearer | — | **200** `{ id, nombre, email, role, profesionalId? }` | 401 |

### Pacientes — solo Admin

| Método | Ruta | Request | Éxito | Errores |
|---|---|---|---|---|
| GET | `/pacientes?search=&page=&pageSize=` | — | **200** `{ items, total, page, pageSize }` | 401 · 403 |
| GET | `/pacientes/{id}` | — | **200** `PacienteDto` | 401 · 403 · 404 |
| POST | `/pacientes` | `{ nombre, apellido, telefono, obraSocial }` | **201** `PacienteDto` + `Location` | 400 · 401 · 403 |
| PUT | `/pacientes/{id}` | `{ nombre, apellido, telefono, obraSocial }` | **200** `PacienteDto` | 400 · 401 · 403 · 404 |
| DELETE | `/pacientes/{id}` | — | **204** (soft delete) | 401 · 403 · 404 · **409** tiene turnos activos |

### Profesionales — escritura solo Admin

| Método | Ruta | Request | Éxito | Errores |
|---|---|---|---|---|
| GET | `/profesionales?search=&page=&pageSize=` | — | **200** `{ items, total, ... }` | 401 · 403 |
| GET | `/profesionales/{id}` | — | **200** `ProfesionalDto` | 401 · 403 · 404 |
| POST | `/profesionales` | `{ nombre, apellido, especialidad, email, password }` | **201** `ProfesionalDto` | 400 · 401 · 403 · **409** email ya registrado |
| PUT | `/profesionales/{id}` | `{ nombre, apellido, especialidad }` | **200** `ProfesionalDto` | 400 · 401 · 403 · 404 |
| DELETE | `/profesionales/{id}` | — | **204** (soft delete) | 401 · 403 · 404 · **409** tiene turnos activos |

> `POST /profesionales` crea el profesional **y** su cuenta de acceso
> (`Rol.Profesional`) en una sola operación — un profesional nunca existe sin
> usuario para loguearse. `email`/`password` no van en `PUT`: editar no
> re-registra credenciales, solo datos (nombre/apellido/especialidad).

### Turnos

| Método | Ruta | Rol | Request | Éxito | Errores |
|---|---|---|---|---|---|
| GET | `/turnos?desde=&hasta=&estado=&pacienteId=&profesionalId=&page=&pageSize=` | Admin (todos) · Profesional (forzado a los suyos) | — | **200** `{ items: TurnoDto[], total, ... }` | 401 |
| GET | `/turnos/{id}` | Admin · Profesional (si es suyo) | — | **200** `TurnoDto` | 401 · **404** (no existe **o** no es suyo) |
| POST | `/turnos` | Admin | `{ pacienteId, profesionalId, inicio, notas? }` | **201** `TurnoDto` + `Location` | 400 · 401 · 403 · 404 paciente/profesional inexistente · **409** slot ocupado |
| PUT | `/turnos/{id}` | Admin | `{ pacienteId, profesionalId, inicio, notas? }` (datos, no estado) | **200** `TurnoDto` | 400 · 401 · 403 · 404 · **409** slot ocupado |
| PATCH | `/turnos/{id}/estado` | Admin (cualquier transición legal) · Profesional (las suyas) | `{ estado }` | **200** `TurnoDto` | 400 · 401 · **404** (no existe o no es suyo) · **409** transición ilegal |

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

| Código | Cuándo |
|---|---|
| **200** | GET / PUT / PATCH ok |
| **201** | POST que crea (con header `Location`) |
| **204** | DELETE, logout |
| **400** | Body mal formado, campos faltantes, `inicio` en el pasado, formato inválido |
| **401** | Sin token / access token expirado o inválido / credenciales malas / refresh inválido |
| **403** | Autenticado pero rol incorrecto (Profesional llamando endpoint solo-Admin) |
| **404** | Recurso inexistente **o** Profesional accediendo a un turno ajeno |
| **409** | Slot ya ocupado · transición de estado ilegal · borrar con turnos activos |
| **500** | Error no controlado |

Cuerpo de error: `ProblemDetails` (RFC 7807).

```jsonc
{
  "type": "https://httpstatuses.io/409",
  "title": "Slot no disponible",
  "status": 409,
  "detail": "El profesional ya tiene un turno el 2026-09-15 a las 15:00.",
  "errors": { }   // presente solo en 400 de validación, por campo
}
```

---

## Autenticación y permisos

### Dos tokens

| | Access token (JWT) | Refresh token |
|---|---|---|
| Vida | ~15 min | ~14 días |
| Formato | JWT HS256 con claims `sub`, `email`, `role`, `profesionalId?` | string opaco aleatorio |
| Dónde vive | solo en memoria del cliente | cookie `httpOnly` `Secure` `SameSite`, `Path=/auth/refresh` |
| En el servidor | no se guarda | se guarda **hasheado** (SHA-256) en `RefreshToken`, revocable |
| Se envía | `Authorization: Bearer` en cada request | automático, solo al endpoint de refresh |

Rotación **revoke-on-use**: cada `/auth/refresh` revoca el token usado y emite uno
nuevo. La detección de reuso completa queda como mejora futura.

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

## Flujos de uso

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

FluentValidation por request, corridas por un filtro global (`ValidationFilter`)
que también revisa los errores de *binding* de ASP.NET Core (JSON malformado,
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
  pura) decide qué flechas son legales; el rol solo acota *sobre qué turnos*
  puede dispararlas cada uno, no *qué transiciones* existen.

---

## Estructura del proyecto

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
├── frontend/                   React + TS + Vite + Tailwind + shadcn/ui
├── docs/                       decisiones técnicas, uso de IA, mejoras futuras
├── scripts/                    smoke.sh — smoke test post-deploy
├── Dockerfile                  build del backend para Railway
├── .dockerignore
└── .github/workflows/          CI (build + test del backend)
```

---

## Decisiones técnicas · Uso de IA · Mejoras futuras

Ver `docs/`:

- [`docs/decisiones-tecnicas.md`](docs/decisiones-tecnicas.md)
- [`docs/uso-de-ia.md`](docs/uso-de-ia.md) — herramientas, prompts principales, qué se revisó/corrigió
- [`docs/mejoras-futuras.md`](docs/mejoras-futuras.md)
