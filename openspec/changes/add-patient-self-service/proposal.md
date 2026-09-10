## Why

Hoy solo el Administrador puede crear turnos. Los requisitos de la prueba piden
roles Administrador y Profesional, pero el usuario quiere sumar un tercer rol
**Paciente** con autoservicio: registrarse, ver la disponibilidad real de un
profesional y sacar / reprogramar / cancelar sus propios turnos sin llamar a la
clínica.

> **Nota de alcance (prueba técnica, deadline sáb 12/09/2026 22:30).** Esto va
> más allá de los requisitos enunciados y agrega ~14–20 h de trabajo (registro
> público, modelo de horarios, cálculo de disponibilidad, UI nueva). Ver
> *Non-goals* y *Estimación*. Si el tiempo aprieta, la recomendación es entregar
> primero el **MVP-Paciente** (registro + ver mis turnos + crear turno contra
> slot libre con 409) y dejar disponibilidad-calculada y reprogramación como
> mejora.

## What Changes

- **Nuevo rol `Paciente`** como tercera opción de `Usuario.Rol`
  (`Admin | Profesional | Paciente`). No se toca nada de lo que hoy pueden hacer
  Admin y Profesional.
- **Auto-registro público**: nuevo endpoint anónimo `POST /auth/register` que
  crea en una transacción `Usuario(Rol=Paciente)` + `Paciente` vinculado 1‑a‑1.
  `login` / `refresh` / `logout` / `me` se reutilizan sin cambios de contrato.
- **JWT del paciente** lleva claim `pacienteId` (análogo a `profesionalId`). El
  cliente nunca envía su `pacienteId`: se lee del token firmado.
- **Turnos — permisos del Paciente** (handler filtra por `pacienteId` del claim,
  igual que hoy con Profesional):
  - `GET /turnos` → forzado a los suyos.
  - `GET /turnos/{id}` → 404 si no es suyo.
  - `POST /turnos` → crea el suyo; `pacienteId` se ignora del body y se toma del
    claim. Estado inicial `Pendiente`. Valida que `inicio` caiga en un slot de
    disponibilidad real del profesional.
  - `PUT /turnos/{id}` → solo puede cambiar `inicio` (reprogramar) y `notas` de
    sus turnos en estado `Pendiente` o `Confirmado`; revalida slot y
    disponibilidad. No puede cambiar `pacienteId` ni `profesionalId`.
  - `PATCH /turnos/{id}/estado` → única transición permitida:
    `Pendiente|Confirmado → Cancelado` sobre sus turnos. No puede `Confirmado`
    ni `Atendido`.
- **Nuevo: disponibilidad de un profesional**:
  `GET /profesionales/{id}/disponibilidad?desde=&hasta=` (autenticado, cualquier
  rol) devuelve la lista de slots libres calculados a partir del horario de
  atención del profesional menos los turnos no cancelados.
- **Nuevo modelo `HorarioProfesional`**: franjas de atención por día de semana
  (`ProfesionalId`, `DiaSemana`, `HoraInicio`, `HoraFin`). Los define el Admin:
  se agregan sub‑rutas `GET/PUT /profesionales/{id}/horarios` (solo Admin).
- **Config de clínica**: `Clinica__SlotMinutos` (default `30`) fija la grilla de
  slots. `Clinica__AntelacionMinimaHoras` (default `24`) y
  `Clinica__MaxTurnosActivosPorPaciente` (default `3`) acotan el autoservicio.
- **Frontend**: pantalla de registro, vista "Mis turnos" del paciente, flujo
  "Sacar turno" (elegir profesional → ver slots libres → confirmar), acciones
  reprogramar / cancelar con la regla de antelación. El menú del paciente no
  muestra Pacientes ni Profesionales ni el alta de turnos de Admin.
- **README**: actualizar auth model (3 roles, claim `pacienteId`), tabla de
  rutas (`/auth/register`, `/profesionales/{id}/disponibilidad`,
  `/profesionales/{id}/horarios`), modelo de datos (`Usuario.PacienteId?`,
  `HorarioProfesional`), máquina de estados (marcar transiciones de Paciente) y
  agregar la sección **Flujos de uso → Paciente**.
- **Seed**: un usuario paciente de demo y horarios de ejemplo para los
  profesionales sembrados.

### Non-goals

- Confirmación de turnos por el paciente (sigue siendo acción de
  Profesional/Admin).
- Detección de reuso de refresh token, verificación de email, recuperación de
  contraseña.
- Notificaciones (email / SMS) de recordatorio o cambio de estado.
- Solape por duración de turno: la colisión sigue siendo por **slot exacto**.
- Excepciones de agenda (feriados, licencias, bloqueos puntuales): el horario es
  solo la grilla semanal recurrente.
- Que el Admin o el Profesional creen usuarios Paciente desde el panel (el alta
  es solo auto-registro).
- Pago / seña del turno.

## Capabilities

### New Capabilities

- `patient-self-registration`: alta pública de un usuario Paciente con su ficha
  de paciente vinculada; unicidad de email; reglas de contraseña.
- `professional-schedule`: horario de atención semanal del profesional
  (franjas por día) administrado por el Admin.
- `appointment-availability`: cálculo y consulta de slots libres de un
  profesional en un rango de fechas, según su horario menos turnos ocupados.
- `patient-appointment-booking`: creación, reprogramación y cancelación por el
  propio paciente de sus turnos, con límites de antelación y de turnos activos.

### Modified Capabilities

<!-- No hay specs en openspec/specs/ todavía; el comportamiento vigente vive en
     el README. Los cambios de contrato sobre auth y turnos quedan capturados en
     las New Capabilities de arriba y en la actualización del README listada en
     Impact. Sin specs previas que versionar como delta. -->

_(ninguna — no hay specs base en `openspec/specs/`; el contrato actual está en
el README y se actualiza como parte de este cambio)_

## Impact

- **Auth**: nuevo endpoint `POST /auth/register`; emisión de JWT con claim
  `pacienteId`; `AuthorizationPolicy` / `[Authorize(Roles="Paciente")]`; el
  enum `Rol` gana un valor. `GET /auth/me` devuelve `pacienteId?`.
- **Turnos**: el handler de `GET /turnos`, `GET /turnos/{id}`, `POST /turnos`,
  `PUT /turnos/{id}` y `PATCH /turnos/{id}/estado` incorpora la rama
  `role == Paciente` (filtro por `pacienteId`, transición única, validación de
  disponibilidad). La regla anti doble-turno (índice único parcial) no cambia.
- **Modelo de datos / EF Core**: `Usuario.PacienteId?` (FK a `Paciente`, único);
  nueva entidad `HorarioProfesional` + `DbSet`; migración nueva; seed ampliado.
- **Config**: `Clinica__SlotMinutos`, `Clinica__AntelacionMinimaHoras`,
  `Clinica__MaxTurnosActivosPorPaciente` en `appsettings` y variables de Railway.
- **Nuevos endpoints**: `GET /profesionales/{id}/disponibilidad`,
  `GET/PUT /profesionales/{id}/horarios`.
- **Frontend**: rutas `/registro` y `/mis-turnos` (paciente) + flujo "Sacar
  turno"; guard de rol en el router; menú condicionado por `role`; cliente HTTP
  sin cambios (mismo interceptor de refresh).
- **Docs**: `README.md` (secciones auth, API, modelo de datos, máquina de
  estados, flujos de uso); `docs/decisiones-tecnicas.md` y `PROMPTS.md`.
- **Tests**: registro (email duplicado → 409, password inválida → 400),
  autorización del rol Paciente (turno ajeno → 404, endpoint Admin → 403),
  transición ilegal → 409, `POST /turnos` fuera de disponibilidad → 409/422,
  antelación mínima al cancelar/reprogramar, límite de turnos activos, y el
  cálculo de disponibilidad (unit).

## Estimación

| Bloque | Estimado |
|---|---|
| Rol + `/auth/register` + claim `pacienteId` + policies | 3–4 h |
| `HorarioProfesional` + endpoints de horarios + migración + seed | 3–4 h |
| Cálculo de disponibilidad + endpoint + tests unit | 3–4 h |
| Ramas del rol Paciente en el handler de turnos + validaciones + tests | 3–4 h |
| Frontend (registro, mis turnos, sacar turno, reprogramar/cancelar) | 4–6 h |
| README + docs + PROMPTS | 1–2 h |
| **Total** | **17–24 h** |

**MVP-Paciente si el tiempo aprieta (~8–10 h):** rol + `/auth/register` + claim,
ramas de `GET /turnos` y `GET /turnos/{id}`, `POST /turnos` contra slot libre
(colisión → 409, sin disponibilidad calculada), `PATCH estado = Cancelado`,
vista "Mis turnos" + "Sacar turno" simple. Disponibilidad calculada,
`HorarioProfesional` y reprogramación quedan como fase 2.
