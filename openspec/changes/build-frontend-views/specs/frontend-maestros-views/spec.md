## ADDED Requirements

### Requirement: Página de pacientes (solo Admin)

La ruta `/pacientes` SHALL estar protegida por `RequireRole` con rol `Admin` y renderizar `PacientesPage`: un `PageHeader` con botón "Nuevo paciente" (`plus`), una barra de búsqueda con ícono `search` y debounce de 300ms, y un `DataTable` alimentado por `GET /pacientes?search=&page=&pageSize=` con columnas Nombre, Apellido, Teléfono, Obra social, Alta (fecha) y un menú de acciones por fila (`more-horizontal`) con "Editar" (`pencil`) y "Eliminar" (`trash-2`, destructivo).

#### Scenario: Acceso denegado a no-Admin

- **WHEN** un usuario `Profesional` navega a `/pacientes`
- **THEN** `RequireRole` lo redirige a `/403`

#### Scenario: Búsqueda con debounce

- **WHEN** el `Admin` escribe en la barra de búsqueda
- **THEN** la petición `GET /pacientes` se dispara 300ms después de la última tecla con el parámetro `search`

#### Scenario: Sin resultados de búsqueda

- **WHEN** la búsqueda no devuelve pacientes
- **THEN** se muestra un `EmptyState` con ícono `user-search` y el texto "No encontramos pacientes con ese texto."

#### Scenario: Sin pacientes cargados

- **WHEN** no existe ningún paciente y no hay búsqueda activa
- **THEN** el `EmptyState` muestra ícono `users`, el texto "Todavía no hay pacientes." y una acción primaria "Nuevo paciente"

### Requirement: Alta y edición de paciente

`PacienteDialog` SHALL crear (`POST /pacientes`) y editar (`PUT /pacientes/{id}`) con los campos Nombre, Apellido, Teléfono (`phone`, con hint de formato) y Obra social (`shield`), validados por `pacienteSchema` (zod) como única fuente de reglas de cliente y del tipo del formulario. Los errores 400 SHALL volcarse por campo desde ProblemDetails.

#### Scenario: Crear paciente exitoso

- **WHEN** el `Admin` completa el formulario con datos válidos y envía
- **THEN** se llama `POST /pacientes`, se cierra el diálogo, se muestra un toast de éxito y la lista se refresca

#### Scenario: Error de validación por campo

- **WHEN** `POST /pacientes` o `PUT /pacientes/{id}` responde 400 con `errors`
- **THEN** cada mensaje aparece bajo su campo mediante `useProblemForm` / `FieldError`

### Requirement: Eliminación de paciente con manejo de 409

Eliminar un paciente SHALL requerir un `ConfirmDialog` (ícono `trash-2` en círculo rosa-tint, texto "No podrás eliminarlo si tiene turnos activos.", botón destructivo "Eliminar") y ejecutar `DELETE /pacientes/{id}`. Un 204 SHALL mostrar un toast "Paciente eliminado" y refrescar la lista. Un 409 SHALL mostrar un toast destructivo (`alert-triangle`) con el `detail` del ProblemDetails y NO eliminar la fila.

#### Scenario: Eliminación exitosa

- **WHEN** el `Admin` confirma la eliminación y el servidor responde 204
- **THEN** se muestra el toast "Paciente eliminado" y la fila desaparece de la lista

#### Scenario: Paciente con turnos activos

- **WHEN** `DELETE /pacientes/{id}` responde 409
- **THEN** se muestra un toast destructivo con ícono `alert-triangle` y el mensaje "Este paciente tiene turnos activos. Cancelá o reasigná esos turnos primero." y el paciente permanece en la lista

### Requirement: Página de profesionales (solo Admin)

La ruta `/profesionales` SHALL estar protegida por `RequireRole` con rol `Admin` y renderizar `ProfesionalesPage` con la misma estructura que `PacientesPage`: `PageHeader` con "Nuevo profesional", búsqueda debounced y `DataTable` desde `GET /profesionales?search=&page=&pageSize=` con columnas Nombre, Apellido, Especialidad, Alta y menú de acciones por fila. El estado vacío usa el ícono `stethoscope`.

#### Scenario: Acceso denegado a no-Admin

- **WHEN** un usuario `Profesional` navega a `/profesionales`
- **THEN** `RequireRole` lo redirige a `/403`

#### Scenario: Listado con resultados

- **WHEN** `GET /profesionales` responde con items
- **THEN** cada fila muestra nombre, apellido, especialidad y fecha de alta, con menú de acciones "Editar" y "Eliminar"

### Requirement: Alta, edición y eliminación de profesional

`ProfesionalDialog` SHALL crear (`POST /profesionales`) y editar (`PUT /profesionales/{id}`) con los campos Nombre, Apellido y Especialidad (`stethoscope`), validados por `profesionalSchema` (zod). La eliminación SHALL comportarse como la de pacientes, incluido el manejo del 409 "tiene turnos activos" con el mensaje adaptado a profesional.

#### Scenario: Crear profesional exitoso

- **WHEN** el `Admin` envía el formulario con datos válidos
- **THEN** se llama `POST /profesionales`, se cierra el diálogo, se muestra un toast de éxito y la lista se refresca

#### Scenario: Profesional con turnos activos

- **WHEN** `DELETE /profesionales/{id}` responde 409
- **THEN** se muestra un toast destructivo con ícono `alert-triangle` y el mensaje "Este profesional tiene turnos activos. Cancelá o reasigná esos turnos primero."
