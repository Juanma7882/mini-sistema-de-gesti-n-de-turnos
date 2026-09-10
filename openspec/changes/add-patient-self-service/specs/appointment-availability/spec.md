## ADDED Requirements

### Requirement: Consulta de slots libres de un profesional

El sistema SHALL exponer
`GET /profesionales/{id}/disponibilidad?desde=&hasta=` para cualquier usuario
autenticado. `desde` y `hasta` SHALL ser fechas (o fecha-hora) locales naïve;
`hasta` SHALL ser posterior a `desde` y el rango NO SHALL exceder 31 días. La
respuesta de éxito SHALL ser **200**
`{ profesionalId, slotMinutos, slots: ["2026-09-15T09:00:00", ...] }` con los
inicios de slot libres ordenados ascendentemente.

#### Scenario: Rango con disponibilidad

- **WHEN** un usuario autenticado consulta la disponibilidad de un profesional
  con horario cargado y algunos turnos ya tomados en el rango
- **THEN** el sistema responde **200** con los inicios de slot que caen dentro
  de una franja de atención y no están ocupados por un turno no cancelado

#### Scenario: Rango inválido

- **WHEN** `hasta <= desde` o el rango supera 31 días
- **THEN** el sistema responde **400** `ProblemDetails`

#### Scenario: Profesional sin horario cargado

- **WHEN** el profesional no tiene ninguna franja de `HorarioProfesional`
- **THEN** el sistema responde **200** con `slots: []`

#### Scenario: Profesional inexistente o dado de baja

- **WHEN** `{id}` no corresponde a un profesional activo
- **THEN** el sistema responde **404**

### Requirement: Definición de la grilla de slots

Los slots SHALL calcularse en pasos de `Clinica__SlotMinutos` (default 30)
comenzando en `HoraInicio` de cada franja. Un slot SHALL ofrecerse solo si el
intervalo `[inicio, inicio + SlotMinutos)` queda íntegramente dentro de una
franja de atención de ese día de semana.

#### Scenario: Slot que no entra completo en la franja

- **WHEN** una franja termina 17:00, `SlotMinutos = 30` y se evalúa el inicio
  16:45
- **THEN** ese inicio NO se incluye en `slots` (16:45–17:15 excede la franja)

### Requirement: Un slot ocupado no se ofrece

Un inicio de slot SHALL excluirse de `slots` si existe un `Turno` para ese
profesional con ese `Inicio` exacto y `Estado <> Cancelado`. Cancelar un turno
SHALL volver a exponer ese slot.

#### Scenario: Slot liberado tras cancelación

- **WHEN** un turno `Confirmado` en un slot se pasa a `Cancelado`
- **THEN** la siguiente consulta de disponibilidad vuelve a incluir ese inicio

### Requirement: Slots en el pasado no se ofrecen

`disponibilidad` SHALL excluir cualquier inicio de slot anterior o igual al
instante actual, incluso si `desde` cae en el pasado.

#### Scenario: desde en el pasado

- **WHEN** `desde` es ayer y `hasta` es mañana
- **THEN** la respuesta solo incluye inicios de slot estrictamente futuros
