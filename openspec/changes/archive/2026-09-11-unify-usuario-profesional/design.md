## Context

`Usuario` (cuenta de acceso: login, rol, `Nombre` para `GET /auth/me`) y
`Profesional` (perfil de negocio: `Nombre`, `Apellido`, `Especialidad`, turnos)
son hoy dos tablas con `Nombre` duplicado y sin invariante de DB que las
mantenga sincronizadas. La FK de la 1:1 vive en `Usuario.ProfesionalId`
(nullable), lo que no refleja la regla real: un `Profesional` **siempre**
necesita un `Usuario`, pero un `Usuario` **no siempre** es `Profesional`
(hoy Admin; a futuro Supervisor/Dueño). Ver [proposal.md](proposal.md) para
la motivación completa y el detalle de "Why ahora".

Puntos del código actual relevantes para el diseño:
- `ProfesionalService.EditarAsync` actualiza `Profesional.Nombre`/`Apellido`
  pero nunca `Usuario.Nombre` → bug de sincronización que dispara este cambio.
- `ProfesionalRepository.GetPagedAsync` usa `AsNoTracking()` y filtra/ordena
  por `p.Nombre`/`p.Apellido` directamente en la query EF.
- `ProfesionalRepository.GetByIdAsync` sí trackea (sin `AsNoTracking`), pero
  no incluye `Usuario` — hace falta para poder mutar `DeletedAt` en `BajaAsync`.
- `IUsuarioRepository` no tiene método `Update`; hasta ahora `Usuario` solo se
  crea, nunca se edita después del alta.

## Goals / Non-Goals

**Goals:**
- Una sola fuente de verdad para el nombre de una persona (`Usuario`).
- La FK de la 1:1 expresa la regla real: `Profesional.UsuarioId` obligatoria.
- Un solo flag de baja/deshabilitación (`Usuario.DeletedAt`), sin dos estados
  independientes que puedan divergir (profesional "activo" con cuenta
  deshabilitada, o viceversa).
- Mantener sin cambios el contrato público de `ProfesionalDto` (mismo JSON
  para el frontend).

**Non-Goals:**
- No normalizar `Especialidad`/`ObraSocial` en tablas (quedan texto libre).
- No agregar roles `Supervisor`/`Dueño` al enum `Rol` en este cambio.
- No migrar datos de producción: es la DB de dev/demo de la prueba técnica:
  se recrea desde el seed, no se escribe un script de migración de datos.

## Decisions

### 1. FK invertida: `Profesional.UsuarioId` (NOT NULL, UNIQUE) reemplaza `Usuario.ProfesionalId`
En una 1:1 con un lado obligatorio y otro opcional, la FK va del lado
obligatorio. Alternativa descartada: mantener `Usuario.ProfesionalId` nullable
y solo mover `Nombre` — se descartó porque no resuelve el problema de fondo
(nada en el schema impediría un `Profesional` sin `Usuario`) y era justamente
lo que motivó revisar esto ahora.

### 2. `Nombre`/`Apellido` viven solo en `Usuario`
`Profesional` deja de tener columnas de nombre; se lee vía `profesional.Usuario.Nombre`/`Apellido`.
Alternativa descartada: mantener `Nombre` en ambas tablas y sincronizar en
`EditarAsync` — se descartó por seguir siendo dos fuentes de verdad con
posibilidad de drift (ej. un futuro cambio de contraseña o de otro campo de
`Usuario` que toque `Nombre` sin pasar por `ProfesionalService`).

### 3. Baja lógica unificada en `Usuario.DeletedAt`
Se elimina `Profesional.DeletedAt`. Alternativa descartada: dos flags
independientes (uno de "cuenta" en `Usuario`, otro de "estado profesional" en
`Profesional`) — se descartó por YAGNI: no hay hoy ningún caso de uso que
necesite que diverjan, y mantenerlos separados solo agrega superficie para
que se desincronicen sin beneficio actual. Si en el futuro aparece un
requisito real (ej. "profesional de licencia sin perder el login"), se separa
en ese momento con el requisito concreto en mano.

**Consecuencia de comportamiento (documentada en proposal.md como BREAKING):**
dar de baja a un profesional ahora también deshabilita su login, porque es el
mismo flag.

### 4. Query filter de baja lógica
`ProfesionalConfiguration` no puede filtrar por una columna que ya no tiene.
El filtro pasa a `HasQueryFilter(p => p.Usuario.DeletedAt == null)`, apoyado en
que `Profesional.UsuarioId` es ahora obligatoria (no hay `Profesional` sin
`Usuario`, así que la navegación nunca es null en runtime). `UsuarioConfiguration`
suma su propio `HasQueryFilter(u => u.DeletedAt == null)` para que Admin/Supervisor
deshabilitados tampoco aparezcan en listados que consulten `Usuario` directamente.

### 5. `ProfesionalRepository` necesita `Include(p => p.Usuario)`
- `GetPagedAsync`: pasa a ordenar/filtrar por `p.Usuario.Nombre`/`Apellido`
  vía `Include` (o proyección con join); dejar `AsNoTracking()` como está.
- `GetByIdAsync`: agrega `Include(p => p.Usuario)` para que `BajaAsync` pueda
  mutar `profesional.Usuario.DeletedAt` sobre una entidad trackeada, sin
  necesitar un método `Update` nuevo en `IUsuarioRepository` (mismo patrón que
  ya usa `EditarAsync` con `Profesional`).

### 6. Migración EF: recrear, no transformar
Como es la SQLite de dev/demo (datos ficticios del seed, sin usuarios reales),
la migración se genera con `dotnet ef migrations add` normal y se aplica sobre
una DB recreada desde `DbSeeder`, en vez de escribir SQL de transformación de
datos existentes. Ahorra tiempo y reduce riesgo dado el deadline.

## Risks / Trade-offs

- **[Riesgo] Cambio de comportamiento en `BajaAsync`** (baja de profesional
  ahora corta el login) podría sorprender si algún test o flujo asumía que un
  profesional dado de baja seguía pudiendo loguearse → **Mitigación**: buscar
  y actualizar esos tests explícitamente en `tasks.md`; documentado como
  BREAKING en proposal.md.
- **[Riesgo] Volumen de archivos tocados** (Domain, Infrastructure, Application,
  Api, seed, ~6 archivos de test) cerca del deadline → **Mitigación**: `tasks.md`
  ordena el trabajo capa por capa para poder parar en un punto compilable si
  el tiempo aprieta; non-goals ya recortan alcance (sin Especialidad/ObraSocial,
  sin roles nuevos).
- **[Trade-off] Toda lectura de nombre de profesional ahora requiere un join**
  a `Usuario` (antes era una columna propia). Aceptado: el volumen de datos de
  esta prueba técnica es mínimo, no hay impacto de performance real.

## Migration Plan

1. Domain: mover `Nombre`/`Apellido`/`DeletedAt` entre `Usuario` y `Profesional`.
2. Infrastructure: actualizar ambas `IEntityTypeConfiguration`, generar nueva
   migración EF, recrear la SQLite local desde el seed actualizado.
3. Application: actualizar repos, `ProfesionalService`, `ProfesionalMapper`,
   DTOs y `CrearProfesionalRequest`.
4. Api: verificar `AuthController`/`GET /auth/me` sigue devolviendo `Nombre`
   correctamente (ahora con `Apellido` disponible si se quisiera exponer,
   fuera de alcance salvo que ya se use).
5. Tests: actualizar fixtures/fakes (`FakeUsuarioRepository`,
   `ApiTestDataExtensions`, `TestData`) y las aserciones que referencian el
   shape viejo.
6. Verificación manual: `dotnet build` + `dotnet test` en verde; smoke test
   manual de alta/edición/baja de profesional contra el frontend ya existente
   (no debería requerir cambios, pero se valida el contrato).

No hay rollback de datos porque no hay datos reales que preservar; ante un
problema, se revierte el commit/branch y se recrea la DB desde el seed previo.

## Open Questions

- Ninguna bloqueante. Si aparece la necesidad real de "profesional inactivo
  con cuenta habilitada" (o viceversa) en el futuro, revisar la Decisión 3.
