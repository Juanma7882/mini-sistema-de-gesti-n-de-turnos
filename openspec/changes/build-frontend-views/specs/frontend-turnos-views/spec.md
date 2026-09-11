## ADDED Requirements

### Requirement: Página de turnos con título contextual

La ruta `/turnos` (accesible a `Admin` y `Profesional`) SHALL renderizar `TurnosPage` con un `PageHeader` cuyo título es "Turnos" para `Admin` y "Mis turnos" para `Profesional`. El botón "Nuevo turno" (ícono `plus`) SHALL mostrarse solo para `Admin`. El frontend SHALL NOT enviar `profesionalId` propio; el servidor filtra por el claim del JWT.

#### Scenario: Título para Admin

- **WHEN** un usuario `Admin` abre `/turnos`
- **THEN** el `PageHeader` muestra "Turnos" y el botón "Nuevo turno"

#### Scenario: Título para Profesional

- **WHEN** un usuario `Profesional` abre `/turnos`
- **THEN** el `PageHeader` muestra "Mis turnos" y no muestra el botón "Nuevo turno"
- **AND** la petición `GET /turnos` no incluye un parámetro `profesionalId` agregado por el cliente

### Requirement: Barra de filtros de turnos sincronizada a la URL

`FiltrosTurnos` SHALL ofrecer filtros de rango de fechas (`desde`, `hasta`), estado, y —solo para `Admin`— profesional y paciente. El filtro de paciente SHALL usar debounce de 300ms. Todos los filtros activos SHALL reflejarse en la query string y rehidratarse al recargar. Un control "Limpiar filtros" (`x`) SHALL aparecer solo cuando hay al menos un filtro activo.

#### Scenario: Filtro reflejado en la URL

- **WHEN** el usuario selecciona un estado y un rango de fechas
- **THEN** la query string incluye `estado`, `desde` y `hasta`
- **AND** al recargar la página los mismos filtros quedan aplicados y visibles

#### Scenario: Filtros exclusivos de Admin

- **WHEN** un usuario `Profesional` ve la barra de filtros
- **THEN** ve rango de fechas y estado, y no ve los filtros de profesional ni de paciente

#### Scenario: Limpiar filtros

- **WHEN** hay al menos un filtro activo y el usuario activa "Limpiar filtros"
- **THEN** se quitan todos los parámetros de filtro de la query string y la lista vuelve al estado por defecto

### Requirement: Tabla de turnos paginada

`TurnosPage` SHALL listar los turnos en un `DataTable` sobre superficie blanca con divisores de fila tipo hairline, columnas Paciente, Profesional, Inicio (fecha + hora vía `formatInicio`, con `tabular-nums`), Estado (badge de color) y Notas (truncadas), más un afford `chevron-right` en la última celda. El pie SHALL mostrar paginación (`‹ ›`, "Página X de Y", tamaño de página). El hover de fila SHALL usar `--color-primary-tint`.

#### Scenario: Render de filas

- **WHEN** `GET /turnos` responde con items
- **THEN** cada fila muestra paciente, profesional, inicio formateado, el badge de estado correspondiente y las notas truncadas

#### Scenario: Estado de carga

- **WHEN** la petición de turnos está en curso
- **THEN** se muestran 6–8 filas skeleton con shimmer en `--color-primary-tint` y la barra de filtros permanece interactiva

#### Scenario: Sin resultados por filtros

- **WHEN** la respuesta no tiene items y hay filtros activos
- **THEN** se muestra un `EmptyState` con ícono `calendar-search`, el texto "No hay turnos para estos filtros." y una acción "Limpiar filtros"

#### Scenario: Sin turnos en absoluto (Admin)

- **WHEN** un `Admin` sin ningún turno abre la lista sin filtros
- **THEN** el `EmptyState` muestra "Todavía no hay turnos." y una acción primaria "Nuevo turno"

#### Scenario: Error al cargar

- **WHEN** `GET /turnos` falla con un error no-500
- **THEN** se muestra un panel inline con ícono `alert-triangle` y un botón "Reintentar"

### Requirement: Drawer de detalle de turno

Al activar una fila, `TurnoDetailDrawer` SHALL abrirse como panel lateral derecho (~420px; sheet a ancho completo en mobile) y reflejar `?turno=<id>` en la URL. SHALL mostrar, separadas por hairline: encabezado (nombre del paciente + badge de estado + cerrar `x`); sección Turno (inicio con `clock`, notas o "Sin notas"); sección Paciente (nombre, teléfono con enlace `tel:` e ícono `phone`, obra social con `shield`); sección Profesional (nombre, especialidad con `stethoscope`).

#### Scenario: Apertura desde una fila

- **WHEN** el usuario hace click o presiona Enter sobre una fila de turno
- **THEN** se abre el drawer con el detalle de ese turno y la URL incluye `?turno=<id>`

#### Scenario: Datos del paciente visibles para el Profesional

- **WHEN** un `Profesional` abre el detalle de un turno propio
- **THEN** ve nombre, teléfono y obra social del paciente

#### Scenario: Turno inaccesible

- **WHEN** el turno referido por `?turno=<id>` responde 404 (inexistente o ajeno)
- **THEN** el drawer muestra un estado con ícono `search-x`, el texto "Este turno ya no está disponible." y una acción "Volver al listado"

### Requirement: Control de transiciones de estado

`EstadoControl` SHALL ofrecer únicamente las transiciones legales para el par (estado actual, rol), obtenidas de `frontend/src/features/turnos/machine.ts` (espejo del backend). Cada transición SHALL ser un botón con ícono: Confirmar `check`, Marcar atendido `clipboard-check`, Cancelar `x-circle` (tinte destructivo). Si no hay transiciones disponibles SHALL mostrarse "Sin acciones disponibles". Cada transición ejecuta `PATCH /turnos/{id}/estado`.

#### Scenario: Transiciones de un Profesional sobre un turno Pendiente

- **WHEN** un `Profesional` abre un turno en estado `Pendiente`
- **THEN** `EstadoControl` ofrece solo "Confirmar" y "Cancelar" y ninguna otra transición

#### Scenario: Estado terminal

- **WHEN** el turno está en `Atendido` o `Cancelado`
- **THEN** `EstadoControl` no ofrece ninguna acción y muestra "Sin acciones disponibles"

#### Scenario: Transición rechazada por el servidor

- **WHEN** `PATCH /turnos/{id}/estado` responde 409 (transición ilegal)
- **THEN** el drawer muestra una alerta inline con el `detail` del ProblemDetails y el estado del turno no cambia en la UI

### Requirement: Alta y edición de turno (solo Admin)

`TurnoDialog` SHALL permitir a un `Admin` crear (`POST /turnos`) y editar (`PUT /turnos/{id}`) un turno. Los campos SHALL ser: paciente (combobox con búsqueda `GET /pacientes?search=`), profesional (`select`), fecha (`calendar` en popover) y hora (selector a precisión de minuto) enviadas como string local naïve, y notas (`textarea`, opcional). El botón de envío SHALL mostrar spinner y "Guardando" durante la petición.

#### Scenario: Crear turno exitoso

- **WHEN** un `Admin` completa el formulario con datos válidos y envía
- **THEN** se llama `POST /turnos`, al resolver se cierra el diálogo, se muestra un toast "Turno creado" (`check-circle`) y la lista se refresca

#### Scenario: Slot ocupado

- **WHEN** `POST /turnos` o `PUT /turnos/{id}` responde 409 slot ocupado
- **THEN** el diálogo muestra una alerta inline arriba del cuerpo con ícono `calendar-x` y el `detail` del ProblemDetails, y el formulario permanece abierto y editable

#### Scenario: Validación por campo

- **WHEN** la respuesta es 400 con `errors` (p. ej. `inicio` en el pasado)
- **THEN** cada error se vuelca al campo correspondiente vía `useProblemForm`

#### Scenario: Paciente o profesional inexistente

- **WHEN** la respuesta es 404 de paciente/profesional
- **THEN** el diálogo muestra una alerta inline "El paciente o profesional seleccionado ya no existe."

### Requirement: Cancelación de turno con confirmación

Cancelar un turno SHALL requerir un `ConfirmDialog` previo (ícono `alert-triangle` en círculo rosa-tint, texto "El horario quedará libre para otro paciente.", botón destructivo "Sí, cancelar"). Al confirmar SHALL ejecutarse `PATCH /turnos/{id}/estado` con `{ estado: "Cancelado" }`, mostrarse un toast "Turno cancelado" y refrescarse la lista.

#### Scenario: Confirmar cancelación

- **WHEN** el usuario activa "Cancelar" en `EstadoControl` y confirma en el diálogo
- **THEN** se llama `PATCH /turnos/{id}/estado` con `estado: "Cancelado"`, se muestra el toast "Turno cancelado" y la lista se refresca liberando el slot

#### Scenario: Abortar cancelación

- **WHEN** el usuario abre el `ConfirmDialog` de cancelación y elige "No, volver"
- **THEN** no se hace ninguna petición y el turno mantiene su estado
