## ADDED Requirements

### Requirement: El Paciente solo ve y accede a sus turnos

Cuando el usuario autenticado tiene `Rol = Paciente`, el handler de `GET /turnos`
SHALL forzar el filtro `t.PacienteId == claims.pacienteId`, ignorando cualquier
`?pacienteId` recibido. `GET /turnos/{id}` SHALL responder **404** si el turno no
pertenece al paciente del claim (mismo criterio "no existe o no es tuyo" que hoy
aplica a Profesional). El `pacienteId` SHALL leerse siempre del JWT, nunca del
cliente.

#### Scenario: Listado forzado a los propios

- **WHEN** un Paciente llama `GET /turnos?pacienteId=<otro>`
- **THEN** la respuesta solo contiene turnos cuyo `paciente.id` es el del claim

#### Scenario: Acceso a turno ajeno por id

- **WHEN** un Paciente llama `GET /turnos/{id}` de un turno de otro paciente
- **THEN** el sistema responde **404**

### Requirement: El Paciente crea su propio turno

`POST /turnos` con `Rol = Paciente` SHALL aceptar `{ profesionalId, inicio, notas? }`,
tomar `pacienteId` del claim (ignorando el del body) y crear el turno en estado
`Pendiente`. El sistema SHALL validar que `inicio` coincida exactamente con un
slot libre de `disponibilidad` del profesional. La regla anti doble-turno
(índice único parcial `(ProfesionalId, Inicio) WHERE Estado <> Cancelado`) se
mantiene y una colisión de carrera SHALL responder **409**.

#### Scenario: Alta válida en slot libre

- **WHEN** un Paciente envía `POST /turnos` con un `inicio` que está en la
  disponibilidad del profesional
- **THEN** el sistema responde **201** `TurnoDto` con `estado = "Pendiente"`,
  `paciente.id` igual al claim y header `Location`

#### Scenario: inicio fuera de la disponibilidad

- **WHEN** el `inicio` no cae en una franja de atención o no está alineado a la
  grilla de slots
- **THEN** el sistema responde **422** `ProblemDetails`
  (`title = "Horario no disponible"`) y no crea el turno

#### Scenario: Slot ya ocupado

- **WHEN** otro turno no cancelado ya existe para ese profesional y ese `inicio`
- **THEN** el sistema responde **409** `ProblemDetails` (`title = "Slot no disponible"`)

#### Scenario: pacienteId del body ignorado

- **WHEN** el body incluye `pacienteId` de otro paciente
- **THEN** el turno se crea para el paciente del claim, no para el del body

#### Scenario: Profesional inexistente o dado de baja

- **WHEN** `profesionalId` no corresponde a un profesional activo
- **THEN** el sistema responde **404**

### Requirement: El Paciente reprograma sus turnos

`PUT /turnos/{id}` con `Rol = Paciente` SHALL permitir cambiar únicamente
`inicio` y `notas` de un turno propio en estado `Pendiente` o `Confirmado`. NO
SHALL permitir cambiar `pacienteId` ni `profesionalId` (si el body los trae
distintos, responde **400**). El nuevo `inicio` SHALL revalidarse contra la
disponibilidad y la regla anti doble-turno, excluyendo el propio `Id`. La regla
de antelación mínima (ver abajo) aplica sobre el `inicio` actual del turno.

#### Scenario: Reprogramación válida

- **WHEN** un Paciente hace `PUT /turnos/{id}` de un turno `Pendiente` suyo con
  un nuevo `inicio` disponible y con más de `AntelacionMinimaHoras` de margen
- **THEN** el sistema responde **200** `TurnoDto` con el nuevo `inicio` y libera
  el slot anterior

#### Scenario: Turno ya Atendido o Cancelado

- **WHEN** el turno está en estado `Atendido` o `Cancelado`
- **THEN** el sistema responde **409** `ProblemDetails` y no lo modifica

#### Scenario: Intento de cambiar el profesional

- **WHEN** el body trae un `profesionalId` distinto al del turno
- **THEN** el sistema responde **400** y no modifica el turno

#### Scenario: Turno ajeno

- **WHEN** el `{id}` es de otro paciente
- **THEN** el sistema responde **404**

### Requirement: El Paciente cancela sus turnos

`PATCH /turnos/{id}/estado` con `Rol = Paciente` SHALL permitir exclusivamente la
transición `Pendiente | Confirmado -> Cancelado` sobre un turno propio. Cualquier
otro `estado` en el body (`Confirmado`, `Atendido`, `Pendiente`) SHALL responder
**409** (`title = "Transición no permitida"`). Un turno ajeno SHALL responder
**404**.

#### Scenario: Cancelación válida

- **WHEN** un Paciente hace `PATCH /turnos/{id}/estado { "estado": "Cancelado" }`
  sobre un turno `Confirmado` suyo con margen suficiente
- **THEN** el sistema responde **200** `TurnoDto` con `estado = "Cancelado"` y el
  slot vuelve a estar disponible

#### Scenario: Paciente intenta confirmar

- **WHEN** el body es `{ "estado": "Confirmado" }`
- **THEN** el sistema responde **409** y el turno no cambia de estado

#### Scenario: Paciente intenta marcar atendido

- **WHEN** el body es `{ "estado": "Atendido" }`
- **THEN** el sistema responde **409**

### Requirement: Antelación mínima para cancelar o reprogramar

El Paciente NO SHALL cancelar ni reprogramar un turno cuyo `inicio` esté a menos
de `Clinica__AntelacionMinimaHoras` (default 24) del instante actual. En ese caso
el sistema SHALL responder **409** `ProblemDetails`
(`title = "Fuera de plazo"`, `detail` con las horas de antelación requeridas).
Esta regla NO aplica al Admin.

#### Scenario: Cancelación tardía

- **WHEN** un Paciente intenta cancelar un turno que empieza dentro de 3 horas y
  `AntelacionMinimaHoras = 24`
- **THEN** el sistema responde **409** y el turno permanece en su estado

#### Scenario: El Admin no tiene esa restricción

- **WHEN** un `Admin` cancela ese mismo turno
- **THEN** el sistema responde **200** con `estado = "Cancelado"`

### Requirement: Límite de turnos activos por paciente

Al crear un turno, si el paciente ya tiene `Clinica__MaxTurnosActivosPorPaciente`
(default 3) turnos en estado `Pendiente` o `Confirmado` con `inicio` futuro, el
sistema SHALL responder **409** `ProblemDetails` (`title = "Límite de turnos alcanzado"`)
y no crear el turno.

#### Scenario: Alta que supera el límite

- **WHEN** un Paciente con 3 turnos activos futuros envía `POST /turnos`
- **THEN** el sistema responde **409** y no crea el cuarto

#### Scenario: Turnos pasados o cancelados no cuentan

- **WHEN** un Paciente tiene 3 turnos `Cancelado` y 1 `Atendido` y ninguno
  `Pendiente`/`Confirmado` futuro
- **THEN** `POST /turnos` responde **201**

### Requirement: El Paciente no accede a la gestión de la clínica

Un usuario con `Rol = Paciente` que llame endpoints restringidos a `Admin`
(`/pacientes/**`, escritura de `/profesionales/**`, `PUT /profesionales/{id}/horarios`,
`POST/PUT /turnos` en nombre de otro, `PATCH estado` con transiciones de gestión)
SHALL recibir **403**. El gating de menú en el frontend es solo UX; la API es la
fuente de verdad.

#### Scenario: Paciente llama endpoint de pacientes

- **WHEN** un Paciente hace `GET /pacientes`
- **THEN** el sistema responde **403**

#### Scenario: Paciente intenta crear un profesional

- **WHEN** un Paciente hace `POST /profesionales`
- **THEN** el sistema responde **403**

### Requirement: `GET /auth/me` incluye `pacienteId` para el rol Paciente

`GET /auth/me` SHALL devolver `pacienteId` cuando el usuario autenticado tiene
`Rol = Paciente`, y `profesionalId` cuando tiene `Rol = Profesional`, de forma
análoga. Para `Admin` ninguno de los dos está presente.

#### Scenario: me de un paciente

- **WHEN** un Paciente autenticado llama `GET /auth/me`
- **THEN** la respuesta incluye `role: "Paciente"` y `pacienteId` con su Id
