## Why

`Usuario` y `Profesional` duplican `Nombre` sin que la DB garantice que queden
sincronizados: `ProfesionalService.EditarAsync` actualiza
`Profesional.Nombre`/`Apellido` pero nunca toca `Usuario.Nombre`, por lo que
`GET /auth/me` puede mostrar un nombre desactualizado después de editar un
profesional. Además, la FK de la relación 1:1 vive del lado equivocado:
`Usuario.ProfesionalId` (nullable) no expresa la regla real del negocio, que es
"un profesional siempre requiere una cuenta de usuario (obligatorio), pero una
cuenta de usuario no siempre pertenece a un profesional" (hoy ya es así con
Admin; a futuro también Supervisor/Dueño). Corregirlo ahora evita arrastrar el
bug de sincronización y una FK invertida a medida que se sumen más roles.

> **Nota de alcance (prueba técnica, deadline sáb 12/09/2026 22:30).** Este
> cambio no lo pide el contrato del README — es una corrección de modelado
> interno. Estimado: 4–6 h (Domain + Infrastructure + migración EF +
> Application + Api + seed + tests existentes a actualizar). Dado el tiempo
> restante, evaluar si conviene ejecutarlo ahora o dejarlo para después de
> entregar el MVP funcional. El contrato público (`ProfesionalDto`) no cambia,
> así que el riesgo para el frontend ya entregado es bajo.

## What Changes

- **Mover `Nombre`/`Apellido` de `Profesional` a `Usuario`.** `Usuario` pasa a
  ser la única fuente de verdad del nombre de una persona (sirve para
  cualquier rol, no solo Profesional).
- **BREAKING (schema): invertir la FK de la relación 1:1.** Hoy
  `Usuario.ProfesionalId` (nullable) apunta a `Profesional`. Pasa a
  `Profesional.UsuarioId` (`NOT NULL`, `UNIQUE`) apuntando a `Usuario`, porque
  el lado obligatorio de una 1:1 es el que debe llevar la FK.
- **BREAKING (comportamiento): unificar la baja lógica.** Se elimina
  `Profesional.DeletedAt`. `Usuario.DeletedAt` pasa a cubrir tanto "cuenta
  deshabilitada" (cualquier rol) como "profesional dado de baja" — son la
  misma fila y, en este dominio, el mismo evento. Dar de baja a un profesional
  ahora también le corta el login (antes no).
- `ProfesionalService.BajaAsync` sigue validando `TieneTurnosActivosAsync`
  antes de permitir la baja, pero persiste el `DeletedAt` en el `Usuario`
  asociado en vez de en el propio `Profesional`.
- `ProfesionalService.CrearAsync`/`EditarAsync`, `ProfesionalMapper`,
  `ProfesionalDto`/`ProfesionalResumenDto`, `CrearProfesionalRequest`,
  `IUsuarioRepository`/`IProfesionalRepository` y sus implementaciones, el
  seed (`DbSeeder`) y los tests existentes se actualizan al nuevo shape.
- **Non-goals (fuera de alcance de este cambio):**
  - No se normalizan `Especialidad` (Profesional) ni `ObraSocial` (Paciente) en
    tablas separadas; quedan como texto libre.
  - No se agregan los roles `Supervisor`/`Dueño` al enum `Rol`. El modelo ya
    los soporta a futuro (Admin ya demuestra que un Usuario puede no tener
    Profesional) sin más cambios de schema.
  - No se agrega ningún endpoint ni campo nuevo visible en la API pública:
    `ProfesionalDto` sigue exponiendo `nombre`/`apellido`/`especialidad`, ahora
    resueltos vía el `Usuario` asociado en vez de columnas propias.

## Capabilities

### New Capabilities
- `professional-account-identity`: reglas de la relación Usuario↔Profesional —
  quién es dueño del nombre de la persona, dirección de la FK, y dónde vive la
  baja lógica/deshabilitación de cuenta. (No existe spec archivada previa para
  esta capability; `openspec/specs/` está vacío todavía.)

### Modified Capabilities
_(ninguna — no hay specs archivadas aún que esta propuesta modifique)_

## Impact

- **Domain**: `Usuario.cs` (+`Apellido`, +`DeletedAt`), `Profesional.cs`
  (−`Nombre`, −`Apellido`, −`DeletedAt`, +`UsuarioId` requerido).
- **Infrastructure**: `UsuarioConfiguration`, `ProfesionalConfiguration`
  (nuevo `HasQueryFilter` vía `Usuario.DeletedAt`, FK invertida), nueva
  migración EF Core (recrea la SQLite de dev/demo — no hay datos de producción
  que migrar).
- **Application**: `ProfesionalService` (Crear/Editar/Baja), `ProfesionalMapper`,
  `ProfesionalDto`, `ProfesionalResumenDto`, `CrearProfesionalRequest`,
  `IUsuarioRepository`, `IProfesionalRepository`, `UsuarioRepository`,
  `ProfesionalRepository`.
- **Api**: `ProfesionalesController` (sin cambio de contrato esperado);
  revisar `AuthController`/`GET /auth/me` por el nuevo `Apellido` en `Usuario`.
- **Seed**: `DbSeeder` — mover `Nombre`/`Apellido` de los `Profesional` seed a
  sus `Usuario`.
- **Tests**: `Api.Tests`, `Application.Tests`, `Infrastructure.Tests` que
  referencian `Profesional.Nombre`/`Apellido`/`DeletedAt` o
  `Usuario.ProfesionalId` (incluye `FakeUsuarioRepository`,
  `ApiTestDataExtensions`, `TestData`).
- **Frontend**: sin cambios de contrato esperados (mismo JSON de
  `ProfesionalDto`); validar manualmente después del cambio.
