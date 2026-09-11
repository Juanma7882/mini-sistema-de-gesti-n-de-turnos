# Professional Account Identity

## Purpose

TBD - captures how a `Profesional` account is identified through its linked
`Usuario`: identity/name ownership, account lifecycle (soft delete), and the
mandatory one-to-one relationship between `Profesional` and `Usuario`.

## Requirements

### Requirement: Un Profesional siempre pertenece a un Usuario
Todo `Profesional` SHALL tener exactamente un `Usuario` asociado
(`Profesional.UsuarioId`, `NOT NULL`, `UNIQUE`). No puede existir un
`Profesional` sin `Usuario`; la creación de ambos SHALL ocurrir en una única
transacción.

#### Scenario: Alta de profesional crea usuario en la misma transacción
- **WHEN** un Admin da de alta un profesional vía `POST /profesionales`
- **THEN** el sistema crea una fila `Profesional` y una fila `Usuario`
  (`Rol = Profesional`) vinculadas, y persiste ambas en un único
  `SaveChanges`

#### Scenario: No puede existir un Profesional sin Usuario
- **WHEN** se intenta insertar una fila en `Profesionales` con `UsuarioId`
  nulo
- **THEN** la base de datos rechaza la operación por la restricción
  `NOT NULL` de `Profesional.UsuarioId`

### Requirement: Un Usuario no requiere ser Profesional
El sistema SHALL permitir cuentas `Usuario` sin `Profesional` asociado (por
ejemplo, rol `Admin`). La ausencia de vínculo a `Profesional` SHALL ser el
estado válido por defecto para cualquier rol que no sea `Profesional`.

#### Scenario: Cuenta Admin sin profesional asociado
- **WHEN** se consulta un `Usuario` con `Rol = Admin`
- **THEN** el sistema no requiere ni expone ningún `Profesional` vinculado
  a esa cuenta

### Requirement: Usuario es la única fuente de verdad del nombre de una persona
`Nombre` y `Apellido` SHALL residir únicamente en `Usuario`. Cualquier
consulta o edición del nombre de un profesional SHALL leer o escribir sobre
el `Usuario` asociado, nunca sobre una columna propia de `Profesional`.

#### Scenario: Editar el nombre de un profesional actualiza su usuario
- **WHEN** un Admin edita `nombre`/`apellido` de un profesional vía
  `PUT /profesionales/{id}`
- **THEN** el cambio se persiste en `Usuario.Nombre`/`Usuario.Apellido` del
  usuario vinculado, y `GET /auth/me` de ese profesional refleja el nombre
  actualizado en el siguiente login o refresh

#### Scenario: Listado de profesionales expone nombre resuelto desde Usuario
- **WHEN** se consulta `GET /profesionales`
- **THEN** cada item del listado incluye `nombre`/`apellido` obtenidos del
  `Usuario` asociado a cada `Profesional`, sin cambiar el contrato JSON
  existente

### Requirement: Baja lógica unificada en Usuario.DeletedAt
La baja lógica y la deshabilitación de cuenta SHALL compartir un único campo,
`Usuario.DeletedAt`. `Profesional` no SHALL tener un campo de baja propio.
Dar de baja a un profesional SHALL deshabilitar simultáneamente su acceso
(login).

#### Scenario: Dar de baja a un profesional sin turnos activos
- **WHEN** un Admin da de baja a un profesional vía `DELETE /profesionales/{id}`
  y ese profesional no tiene turnos en estado `Pendiente` o `Confirmado`
- **THEN** el sistema marca `Usuario.DeletedAt` (UTC actual) en el usuario
  asociado, y ese usuario deja de poder autenticarse

#### Scenario: No se puede dar de baja con turnos activos
- **WHEN** un Admin intenta dar de baja a un profesional que tiene al menos
  un turno en estado `Pendiente` o `Confirmado`
- **THEN** el sistema responde `409 Conflict` y no modifica `Usuario.DeletedAt`

#### Scenario: Dar de baja a un profesional ya dado de baja
- **WHEN** un Admin intenta dar de baja a un profesional cuyo usuario ya
  tiene `DeletedAt` seteado
- **THEN** el sistema responde `404 Not Found`, porque el query filter global
  ya excluye a los profesionales con `Usuario.DeletedAt` no nulo de
  `GetByIdAsync` (mismo comportamiento que antes de este cambio, cuando el
  filtro corría sobre `Profesional.DeletedAt`)

#### Scenario: Profesional dado de baja no aparece en listados activos
- **WHEN** se consulta `GET /profesionales` o `GET /profesionales/{id}`
  para un profesional cuyo `Usuario.DeletedAt` no es nulo
- **THEN** el sistema lo excluye del listado (query filter global) y
  devuelve `404` en la consulta por id
