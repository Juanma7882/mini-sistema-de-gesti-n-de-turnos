# Tasks — add-patient-self-service

Orden por camino crítico. Los grupos **1–5** son el **MVP-Paciente** (compila,
corre y demuestra el rol). Los grupos **6–8** son la fase 2 (disponibilidad,
horarios, reprogramación). El grupo **9** (frontend) y el **10** (docs) cierran.

## 1. Modelo de datos y migración (MVP)

- [ ] 1.1 Agregar `Paciente = 2` al enum `Rol` en Domain.
- [ ] 1.2 Agregar `PacienteId?` (FK a `Paciente`, índice único) a la entidad `Usuario` y su configuración EF.
- [ ] 1.3 Confirmar índice único en `Usuario.Email` (crearlo si no existe en la config EF).
- [ ] 1.4 Generar migración `AddPatientRole` y verificar `dotnet ef database update` sobre una base limpia.
- [ ] 1.5 Ampliar el seed: 1 usuario `paciente@clinica.test` (rol Paciente) enlazado a un `Paciente` demo, solo si la base está vacía.

## 2. Auth: claim pacienteId y /auth/me (MVP)

- [ ] 2.1 En el generador de JWT, emitir claim `pacienteId` cuando `Rol == Paciente` (simétrico a `profesionalId`).
- [ ] 2.2 Extender el lector de claims / `CurrentUser` para exponer `PacienteId?`.
- [ ] 2.3 `GET /auth/me`: devolver `pacienteId` para rol Paciente (y mantener `profesionalId` para Profesional).
- [ ] 2.4 Tests: `me` de un paciente incluye `role="Paciente"` y `pacienteId`.

## 3. Auth: POST /auth/register (MVP)

- [ ] 3.1 DTO `RegisterRequest { email, password, nombre, apellido, telefono, obraSocial }` + validador (email formato, password mín. 8 con letra y dígito, campos de paciente obligatorios, teléfono con formato).
- [ ] 3.2 Caso de uso `RegisterPaciente`: transacción `INSERT Paciente` + `INSERT Usuario(Rol=Paciente, PasswordHash=BCrypt, PacienteId)`; rollback total ante error.
- [ ] 3.3 Ignorar cualquier campo de rol del body; forzar `Rol=Paciente`.
- [ ] 3.4 Manejo de email duplicado: chequeo previo + captura de violación de índice único → **409** `ProblemDetails`.
- [ ] 3.5 Endpoint anónimo `POST /auth/register` → **201** `{ token, user }` + `Set-Cookie: rt` (reusar emisión de tokens de `login`).
- [ ] 3.6 Tests: alta OK (201 + cookie + pacienteId), email duplicado (409), password inválida (400), datos incompletos (400), rol en body ignorado, hash BCrypt presente.

## 4. Turnos: rama del rol Paciente — lectura y creación (MVP)

- [ ] 4.1 `GET /turnos`: agregar rama `role == Paciente` → forzar `where t.PacienteId == claim.pacienteId`, ignorar `?pacienteId`.
- [ ] 4.2 `GET /turnos/{id}`: **404** si el turno no es del paciente del claim.
- [ ] 4.3 `POST /turnos` para Paciente: aceptar `{ profesionalId, inicio, notas? }`, tomar `pacienteId` del claim (ignorar el del body), estado inicial `Pendiente`; mantener regla anti doble-turno (409 en colisión/carrera).
- [ ] 4.4 Validar profesional existente y activo → **404** si no.
- [ ] 4.5 Tests: listado forzado a los propios, turno ajeno → 404, alta OK (201 con paciente del claim), pacienteId del body ignorado, slot ocupado → 409, profesional inexistente → 404.

## 5. Turnos: cancelación por el Paciente + autorización (MVP)

- [ ] 5.1 Tabla de transiciones permitidas por rol; para Paciente solo `Pendiente|Confirmado → Cancelado`.
- [ ] 5.2 `PATCH /turnos/{id}/estado` para Paciente: aplicar la única transición; cualquier otro `estado` → **409**; turno ajeno → **404**.
- [ ] 5.3 Confirmar que `[Authorize(Roles="Admin")]` en `/pacientes/**` y escritura de `/profesionales/**` devuelve **403** para Paciente (agregar tests, no debería requerir código nuevo).
- [ ] 5.4 Tests: cancelación OK (200 + slot liberado), Paciente intenta Confirmado → 409, intenta Atendido → 409, `GET /pacientes` como Paciente → 403, `POST /profesionales` como Paciente → 403.

## 6. Config de clínica y reglas de autoservicio (fase 2)

- [ ] 6.1 `ClinicaOptions { SlotMinutos=30, AntelacionMinimaHoras=24, MaxTurnosActivosPorPaciente=3 }` + bind desde `appsettings` (`Clinica__*`).
- [ ] 6.2 Regla "antelación mínima" en cancelar/reprogramar del Paciente → **409** `Fuera de plazo`; Admin exento.
- [ ] 6.3 Regla "límite de turnos activos" en `POST /turnos` del Paciente (Pendiente/Confirmado futuros) → **409** `Límite de turnos alcanzado`.
- [ ] 6.4 Inyectar `IClock` (hora de clínica) para comparaciones "ahora"; usarlo en las reglas y en disponibilidad.
- [ ] 6.5 Tests: cancelación tardía → 409, Admin cancela el mismo turno → 200, 4º turno activo → 409, turnos pasados/cancelados no cuentan.

## 7. HorarioProfesional (fase 2)

- [ ] 7.1 Entidad `HorarioProfesional { Id, ProfesionalId, DiaSemana(0..6), HoraInicio, HoraFin }` + `DbSet` + converter `TimeOnly`↔`TEXT`.
- [ ] 7.2 Migración `AddProfessionalSchedule` + seed de franjas para los profesionales demo (Lun–Vie 09:00–13:00 y 14:00–18:00).
- [ ] 7.3 Validación: `HoraInicio < HoraFin`; sin solape entre franjas del mismo `DiaSemana` → **400**.
- [ ] 7.4 `GET /profesionales/{id}/horarios` (cualquier rol autenticado) → 200 ordenado por `diaSemana, horaInicio`; profesional inexistente → 404.
- [ ] 7.5 `PUT /profesionales/{id}/horarios` (solo Admin) reemplaza el set completo → 200; otros roles → 403.
- [ ] 7.6 Tests: define horario OK, horas invertidas → 400, franjas solapadas → 400, Profesional/Paciente PUT → 403.

## 8. Disponibilidad y validación de slot (fase 2)

- [ ] 8.1 Función pura `AvailabilityCalculator.FreeSlots(horarios, turnosOcupados, desde, hasta, slotMinutos, ahora)` → `IEnumerable<DateTime>` (grilla por franja, excluye ocupados con `Estado<>Cancelado`, excluye `<= ahora`).
- [ ] 8.2 Tests unitarios del calculador: slot que no entra completo en la franja se excluye; slot liberado tras cancelación reaparece; `desde` en el pasado no devuelve slots pasados; profesional sin horario → vacío.
- [ ] 8.3 `GET /profesionales/{id}/disponibilidad?desde=&hasta=` (autenticado): validar rango (`hasta>desde`, ≤ 31 días → 400), profesional activo (404), responder `{ profesionalId, slotMinutos, slots }`.
- [ ] 8.4 `POST /turnos` del Paciente: exigir que `inicio` caiga en un slot de disponibilidad → si no, **422** `Horario no disponible`. Admin exento de este chequeo.
- [ ] 8.5 `PUT /turnos/{id}` del Paciente: permitir cambiar solo `inicio` y `notas` de turnos propios `Pendiente|Confirmado`; revalidar disponibilidad + anti doble-turno (excluyendo el propio Id); cambiar `pacienteId`/`profesionalId` → **400**; turno `Atendido`/`Cancelado` → **409**; turno ajeno → **404**.
- [ ] 8.6 Tests: reprogramación OK (200 + libera slot viejo), inicio fuera de disponibilidad → 422, intento de cambiar profesional → 400, turno terminal → 409, turno ajeno → 404.

## 9. Frontend (fase 2)

- [ ] 9.1 Página pública `/registro` (form + validación) → `POST /auth/register` → entra logueado a `/mis-turnos`.
- [ ] 9.2 Guard `RequireRole(["Paciente"])` y menú condicionado por `role` (sin Pacientes/Profesionales/alta de turnos Admin); snapshot `{nombre, role}` en `localStorage` cubre Paciente.
- [ ] 9.3 Vista `/mis-turnos`: listado del paciente (server-filtrado) con filtros rango de fechas y estado; acción Cancelar (respetando antelación, error inline).
- [ ] 9.4 Flujo `/sacar-turno`: elegir profesional → `GET /disponibilidad` (próximos 14 días) → grilla de slots → `POST /turnos`; manejar 409/422 inline.
- [ ] 9.5 Acción Reprogramar desde un turno: reusar la grilla de slots → `PUT /turnos/{id}`.
- [ ] 9.6 Smoke manual del recorrido completo: registro → sacar turno → reprogramar → cancelar.

## 10. Documentación y cierre

- [ ] 10.1 README: auth model (3 roles, claim `pacienteId`, hidratación), tabla de rutas (`/auth/register`, `/profesionales/{id}/disponibilidad`, `/profesionales/{id}/horarios`), modelo de datos (`Usuario.PacienteId?`, `HorarioProfesional`), status codes (agregar 422), máquina de estados (marcar transiciones de Paciente).
- [ ] 10.2 README: nueva sección **Flujos de uso → Paciente** y credenciales demo del paciente.
- [ ] 10.3 `docs/decisiones-tecnicas.md`: registrar D1–D9 (enum vs auth separada, 422, antelación/límite configurables, disponibilidad calculada al vuelo).
- [ ] 10.4 `PROMPTS.md`: agregar los prompts principales usados en este cambio y qué se revisó/corrigió.
- [ ] 10.5 Verificar CI verde (build + test) y correr `openspec validate add-patient-self-service`.
