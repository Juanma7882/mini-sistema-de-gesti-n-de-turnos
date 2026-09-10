## ADDED Requirements

### Requirement: Horario de atención semanal del profesional

El sistema SHALL modelar el horario de atención de un profesional como un
conjunto de franjas recurrentes por día de semana
(`HorarioProfesional { Id, ProfesionalId, DiaSemana (0..6), HoraInicio, HoraFin }`).
`HoraInicio` y `HoraFin` SHALL ser horas locales naïve con precisión de minuto y
`HoraInicio < HoraFin`. Puede haber varias franjas por día (p. ej. mañana y
tarde). Las franjas de un mismo día NO SHALL solaparse entre sí.

#### Scenario: Franja con horas invertidas

- **WHEN** se guarda una franja con `HoraFin <= HoraInicio`
- **THEN** el sistema responde **400** y no persiste la franja

#### Scenario: Franjas solapadas en el mismo día

- **WHEN** se guardan dos franjas para el mismo `DiaSemana` cuyos rangos se
  solapan
- **THEN** el sistema responde **400** `ProblemDetails` indicando el solape

### Requirement: Administración de horarios (solo Admin)

El sistema SHALL exponer `GET /profesionales/{id}/horarios` (cualquier rol
autenticado) y `PUT /profesionales/{id}/horarios` (solo `Admin`). El `PUT` SHALL
reemplazar el conjunto completo de franjas del profesional con la lista enviada
`[{ diaSemana, horaInicio, horaFin }]` y responder **200** con la lista
resultante.

#### Scenario: Admin define el horario

- **WHEN** un `Admin` envía `PUT /profesionales/{id}/horarios` con una lista
  válida de franjas
- **THEN** el sistema reemplaza las franjas del profesional y responde **200**
  con la lista guardada

#### Scenario: Profesional intenta editar su horario

- **WHEN** un usuario con rol `Profesional` o `Paciente` llama
  `PUT /profesionales/{id}/horarios`
- **THEN** el sistema responde **403**

#### Scenario: Profesional inexistente

- **WHEN** `{id}` no corresponde a un profesional activo
- **THEN** el sistema responde **404**

#### Scenario: Consulta de horario

- **WHEN** un usuario autenticado llama `GET /profesionales/{id}/horarios`
- **THEN** el sistema responde **200** con las franjas del profesional ordenadas
  por `diaSemana`, `horaInicio`

### Requirement: Reducir el horario no borra turnos existentes

Editar el horario de atención SHALL afectar solo el cálculo de disponibilidad
futura. Los turnos ya agendados que queden fuera del nuevo horario SHALL
permanecer sin cambios (no se cancelan automáticamente).

#### Scenario: Turno queda fuera del nuevo horario

- **WHEN** un `Admin` recorta una franja y existía un turno `Pendiente` dentro
  del tramo eliminado
- **THEN** el turno sigue existiendo con su estado y ese instante deja de
  ofrecerse como slot libre en `disponibilidad`
