## Context

El sistema (aún en fase de diseño, contrato en `README.md`) tiene dos roles:
`Admin` (todo) y `Profesional` (solo sus turnos, filtrado en el handler por el
claim `profesionalId`). El JWT es propio (HS256), con refresh token en cookie
`httpOnly` y rotación revoke-on-use. La colisión de turnos es por **slot exacto**
`(ProfesionalId, Inicio)` garantizada por un índice único parcial
`WHERE Estado <> Cancelado` + validación en Application.

Este cambio agrega un tercer rol `Paciente` con autoservicio. La pieza no trivial
es la **disponibilidad**: hoy el Admin sabe qué horario poner; el paciente
necesita que el sistema le ofrezca slots. Eso obliga a modelar el horario de
atención del profesional.

Restricción dura: prueba técnica con deadline **sáb 12/09/2026 22:30**. El diseño
prioriza reutilizar lo existente (mismo login, mismo patrón de filtrado por
claim, misma regla de slot) y aísla lo nuevo para poder recortar a un
**MVP-Paciente** si el tiempo aprieta (ver `proposal.md`).

## Goals / Non-Goals

**Goals:**

- Sumar `Paciente` sin tocar el comportamiento actual de `Admin` / `Profesional`.
- Reutilizar el patrón "claim → filtro en el handler": `pacienteId` es a
  `Paciente` lo que `profesionalId` es a `Profesional`.
- Registro público atómico (usuario + ficha de paciente en una transacción).
- Disponibilidad calculada de forma determinista y testeable (función pura sobre
  franjas + turnos ocupados).
- Que la API sea la única frontera de seguridad; el frontend solo oculta.

**Non-Goals:**

- Verificación de email, recuperación de contraseña, detección de reuso de
  refresh token.
- Notificaciones / recordatorios.
- Solape por duración de turno (sigue siendo slot exacto).
- Excepciones de agenda (feriados, licencias): solo grilla semanal recurrente.
- Alta de pacientes-usuario por Admin/Profesional (solo auto-registro).
- Multi-clínica / múltiples zonas horarias (hora local naïve, una clínica).

## Decisions

### D1 — `Paciente` como valor del enum `Rol`, no un modelo de auth separado

`Rol { Admin = 0, Profesional = 1, Paciente = 2 }`. Un solo `Usuario`, un solo
`login`, un solo pipeline de JWT.
**Alternativa descartada:** tabla/tokens separados para pacientes → duplica auth,
refresh y middleware para cero beneficio.
**Consecuencia:** el claim de identidad de recurso es polimórfico: el JWT lleva
`profesionalId` **o** `pacienteId` según el rol. `GET /auth/me` refleja el que
corresponda.

### D2 — Vínculo `Usuario` ⇆ `Paciente` 1‑a‑1 vía `Usuario.PacienteId?`

FK anulable y **única** en `Usuario` (un paciente-ficha ↔ a lo sumo un usuario).
Coherente con `Usuario.ProfesionalId?` ya existente.
**Alternativa descartada:** `UsuarioId?` en `Paciente` → deja la FK en la tabla
de dominio y complica el CRUD de pacientes del Admin. Mantener ambas FKs juntas
en `Usuario` centraliza "qué recurso representa este login".
**Nota:** los pacientes creados por el Admin (CRUD actual) siguen sin usuario;
solo los del registro público quedan enlazados.

### D3 — `POST /auth/register` anónimo y transaccional

Un endpoint nuevo en el mismo controlador de auth. Flujo:
1. Validar body (email formato + único, password política, datos de paciente).
2. `BEGIN TRANSACTION` → `INSERT Paciente` → `INSERT Usuario(Rol=Paciente, PasswordHash=BCrypt(pwd), PacienteId=<nuevo>)` → `COMMIT`.
3. Emitir access token + refresh cookie (misma función que usa `login`).
Rol siempre `Paciente`: cualquier campo de rol del body se ignora.
**Alternativa descartada:** registro en dos pasos (crear usuario, luego ficha) →
ventana de inconsistencia y estados intermedios inválidos.
**Colisión de email:** capturar violación de índice único → **409** (además del
chequeo previo, para la carrera).

### D4 — `HorarioProfesional`: franjas recurrentes por día de semana

`HorarioProfesional { Id, ProfesionalId (FK), DiaSemana (0..6, 0 = domingo), HoraInicio (TimeOnly), HoraFin (TimeOnly) }`.
Varias franjas por día; sin solape entre franjas del mismo día (validado en
Application). Administrado por el Admin con
`GET /profesionales/{id}/horarios` (cualquier rol) y
`PUT /profesionales/{id}/horarios` (Admin) que **reemplaza el set completo**
(más simple y predecible que PATCH incremental para este alcance).
**Alternativa descartada:** agenda con fechas concretas / slots materializados en
tabla → mucho más pesado; los slots se calculan al vuelo.
**Persistencia SQLite:** `TimeOnly` se mapea a `TEXT` "HH:mm:ss" (converter EF).

### D5 — Disponibilidad calculada al vuelo por una función pura

`AvailabilityCalculator.FreeSlots(horarios, turnosOcupados, desde, hasta, slotMinutos, ahora)`
→ `IEnumerable<DateTime>`. Algoritmo: para cada día del rango, tomar las franjas
de ese `DiaSemana`; generar inicios en pasos de `slotMinutos` desde `HoraInicio`
mientras `inicio + slotMinutos <= HoraFin`; excluir los que ya tienen turno con
`Estado <> Cancelado`; excluir `inicio <= ahora`. Endpoint
`GET /profesionales/{id}/disponibilidad?desde=&hasta=` (autenticado, cualquier
rol; el rango se limita a 31 días).
**Alternativa descartada:** tabla de slots materializada + job → sincronización y
migraciones extra, innecesario a esta escala.
**Ventaja:** función pura → tests unitarios directos, sin BD.

### D6 — El rol `Paciente` reusa el patrón de filtrado por claim del `Profesional`

En el handler de turnos, `switch(role)`:
- `Admin` → sin filtro (comportamiento actual).
- `Profesional` → `where t.ProfesionalId == claim.profesionalId` (actual).
- `Paciente` → `where t.PacienteId == claim.pacienteId` (nuevo, simétrico).
Acceso por id a recurso ajeno → **404** (mismo criterio que hoy). Endpoints
solo-Admin → **403** vía `[Authorize(Roles = "Admin")]`.
**Alternativa descartada:** middleware de "ownership" genérico → sobre-ingeniería
para dos ramas.

### D7 — Validación de `POST/PUT /turnos` del Paciente contra disponibilidad

Antes de insertar/actualizar: (a) `inicio` alineado a la grilla y dentro de una
franja del profesional (reusa `AvailabilityCalculator`); si no → **422**
`Horario no disponible`. (b) slot libre: chequeo en Application + índice único
parcial; carrera → captura `DbUpdateException` → **409**. El Admin **no** pasa
por (a): puede agendar fuera de horario si quiere (es gestión).
**422 vs 400:** el body está bien formado pero la regla de negocio "ese horario
no se ofrece" no se cumple → 422 (Unprocessable). Se documenta en el README.

### D8 — Reglas de autoservicio configurables

`Clinica__SlotMinutos = 30`, `Clinica__AntelacionMinimaHoras = 24`,
`Clinica__MaxTurnosActivosPorPaciente = 3`. Bind a `ClinicaOptions`
(`IOptions<>`). Defaults sensatos, ajustables por variable de entorno en Railway
sin recompilar. La antelación mínima y el límite de activos **solo** aplican al
rol Paciente; el Admin queda exento.

### D9 — Transiciones de estado del Paciente

Se agrega una tabla de transiciones permitidas por rol. Para `Paciente` la única
arista es `Pendiente | Confirmado → Cancelado`. La máquina de estados global no
cambia; el rol solo restringe qué aristas legales puede disparar cada quien
(igual que ya ocurre con `Profesional`). Cualquier otra → **409**.

### D10 — Frontend: guard por rol + menú condicionado

Router con `RequireRole(["Paciente"])` para `/mis-turnos` y `/sacar-turno`;
`/registro` público. El snapshot `{ nombre, role }` de `localStorage` (ya
previsto para Admin/Profesional) ahora también cubre `Paciente` para pintar el
menú correcto sin flash. Cliente HTTP e interceptor de refresh sin cambios.
Flujo "Sacar turno": elegir profesional → `GET /disponibilidad` (próximos 14
días) → grilla de slots → `POST /turnos`.

## Risks / Trade-offs

- **[Alcance vs deadline]** El set completo son ~17–24 h en una prueba con 2 días
  → *Mitigación:* MVP-Paciente definido en `proposal.md` (sin disponibilidad
  calculada ni `HorarioProfesional` ni reprogramación); el diseño aísla esos
  bloques para poder cortarlos sin refactor.
- **[Zona horaria]** Todo es hora local naïve; el cálculo de slots usa `DateTime`
  sin offset. Si el server de Railway corre en UTC, "ahora" debe compararse en la
  misma referencia → *Mitigación:* fijar `TZ` del contenedor o inyectar un
  `IClock` que devuelva hora de clínica; cubierto por test.
- **[Carrera en reserva]** Dos pacientes reservando el mismo slot →
  *Mitigación:* el índice único parcial ya existe; capturar `DbUpdateException`
  → 409. Test de concurrencia.
- **[Carrera en registro]** Dos registros con el mismo email →
  *Mitigación:* índice único en `Usuario.Email` + captura → 409.
- **[Reducción de horario deja turnos "huérfanos"]** Un turno puede quedar fuera
  del horario nuevo → *Mitigación decidida:* no se cancela nada automáticamente;
  solo deja de ofrecerse el slot (documentado en el spec).
- **[422 poco convencional]** Introduce un status nuevo respecto del README
  actual → *Mitigación:* documentarlo en la tabla de status codes; alternativa
  400 si se prefiere consistencia.
- **[Límite de turnos activos molesto en demo]** `Max = 3` puede estorbar al
  probar → *Mitigación:* es configurable; subirlo en el entorno de demo.

## Migration Plan

1. **EF Core**: agregar `Rol.Paciente`, `Usuario.PacienteId?` (FK única),
   entidad `HorarioProfesional` + `DbSet`. Generar **una** migración
   `AddPatientRoleAndProfessionalSchedule`. `dotnet ef database update` en el
   arranque (o en deploy) sobre el volume de Railway.
2. **Config**: agregar `Clinica__*` a `appsettings.json` (defaults) y a las
   variables de Railway.
3. **Seed**: si la base está vacía, sembrar 1 usuario paciente demo
   (`paciente@clinica.test`) enlazado a un `Paciente`, y franjas de
   `HorarioProfesional` para los profesionales sembrados (p. ej. Lun–Vie
   09:00–13:00 y 14:00–18:00).
4. **Deploy**: backend primero (contrato retrocompatible: los endpoints nuevos no
   afectan a los clientes actuales), luego frontend.
5. **Rollback**: como no hay datos productivos, revertir la migración
   (`ef migrations remove` / `database update <anterior>`) y desplegar el commit
   previo. `Usuario.PacienteId` y `HorarioProfesional` quedan sin uso si se
   revierte solo el código.

## Open Questions

- ¿`422` para "horario no disponible" o preferís `400`/`409` por consistencia con
  el README actual? (el diseño asume 422).
- ¿La antelación mínima (24 h) y el límite de activos (3) van con esos valores
  para la entrega, o los dejamos más laxos para facilitar la demo?
- ¿El endpoint de disponibilidad requiere auth (asumido) o puede ser público para
  que un visitante vea horarios antes de registrarse?
- ¿Rango por defecto que muestra el frontend al "Sacar turno": 14 días? ¿30?
- ¿Se siembra más de un paciente demo para mostrar el aislamiento entre
  pacientes en la evaluación?
