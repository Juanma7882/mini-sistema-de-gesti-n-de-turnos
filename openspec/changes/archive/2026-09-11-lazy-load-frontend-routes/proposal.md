## Why

El router (`src/app/router.tsx`) importa todas las páginas de forma estática, así que el bundle inicial de cualquier usuario —incluido un Profesional, que solo puede ver `/turnos`— incluye también `PacientesPage` y `ProfesionalesPage` (rutas exclusivas de Admin) más sus diálogos de alta/edición. Separar eso en chunks aparte reduce el JS que se descarga y parsea en el primer load sin tocar ninguna regla de negocio, y es una mejora barata de dejar documentada para la entrega.

## What Changes

- Code-splitting por ruta en `src/app/router.tsx`: `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage` y `NotFoundPage` pasan a cargarse con `React.lazy` (via el campo `lazy` de `createBrowserRouter`, no `React.lazy` + `element` manual, para no perder el data-router de react-router). `LoginPage` y `TurnosPage` quedan estáticas: son la pantalla de entrada y la home de todo usuario logueado, lazy-loadearlas no ahorra nada y solo suma un flash de loading.
- Un `<Suspense>` (o el `HydrateFallback` del router) reusa el `FullScreenSpinner` que ya existe para `auth.status === 'loading'`, para que el fallback de carga de ruta se vea igual al que ya conoce el usuario.
- Non-goal explícito por el deadline (48 h, entrega 2026-09-12 22:30): NO se lazy-cargan los diálogos/drawers pesados (`TurnoDialog`, `PacienteDialog`, `ProfesionalDialog`, `TurnoDetailDrawer`) en este change. Quedan como trabajo futuro anotado en `tasks.md` de esta propuesta si sobra tiempo, pero no bloquean el cierre del change.
- Non-goal: no se agrega prefetching (`<Link prefetch>`, hover-preload) ni un router config nuevo (seguimos con `createBrowserRouter`, sin migrar a otra librería).

## Capabilities

### New Capabilities
- `frontend-route-code-splitting`: define qué rutas se cargan de forma diferida, con qué fallback visual, y las garantías de comportamiento (guards de rol/auth siguen ejecutando antes de descargar el chunk; sin flash de contenido no autorizado).

### Modified Capabilities
(ninguna — no hay specs previas de routing/shell con requisitos formales que cambien; `frontend-app-shell` de `build-frontend-views` cubre layout/navbar, no code-splitting)

## Impact

- Código: `frontend/src/app/router.tsx` (definición de rutas), posiblemente `frontend/src/shared/components/FullScreenSpinner.tsx` (reuso, sin cambios) o `core/layouts/` si se necesita un wrapper de fallback.
- Build: Vite ya soporta `import()` dinámico out of the box (Rollup code-splitting); no hay dependencias nuevas.
- Tests: `frontend/src/app/router.tsx` no tiene tests dedicados hoy; verificar manualmente que `/pacientes` y `/profesionales` sigan protegidas por `RequireRole` y que `pnpm build` genere chunks separados (`dist/assets/PacientesPage-*.js`, etc.).
- Estimado: ~1–1.5 h (cambio acotado a una sola sección de un archivo + verificación de build/manual). Cabe cómodo en el presupuesto de 48 h sin desplazar tareas pendientes de `build-frontend-views` (§1.4, §2.2, §8.1, §8.4–8.6).
