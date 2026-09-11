## 1. Fallback visual

- [ ] 1.1 Crear `frontend/src/shared/components/RouteFallback.tsx`: mismo lenguaje visual que `FullScreenSpinner` (`Loader2` de lucide, `animate-spin`, `text-primary`, `role="status" aria-label="Cargando"`) pero sin `min-h-dvh` — pensado para ocupar el área de contenido dentro de `MainLayout`, no el viewport completo.

## 2. Code-splitting en el router

- [ ] 2.1 En `frontend/src/app/router.tsx`, reemplazar el import estático de `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage` y `NotFoundPage` por `React.lazy(() => import('...'))`.
- [ ] 2.2 Envolver cada uno de esos 4 `element` con `<Suspense fallback={<RouteFallback />}>...</Suspense>` (un `Suspense` por ruta, no uno compartido — ver `design.md` Decisión 2). Dejar `LoginPage` y `TurnosPage` con import estático, sin Suspense.
- [ ] 2.3 Confirmar que `RequireRole`/`RequireAuth` siguen envolviendo las rutas lazy en el mismo punto del árbol que antes (el `Suspense` va *dentro* del guard, no en vez de él).

## 3. Verificación

- [ ] 3.1 `pnpm typecheck && pnpm lint` en verde.
- [ ] 3.2 `pnpm build`; inspeccionar `dist/assets/` (o el resumen que imprime Vite) y confirmar que aparecen chunks separados para `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage` y `NotFoundPage`, distintos del chunk principal.
- [ ] 3.3 `pnpm preview` (o `pnpm dev`) y verificar manualmente con Network throttling (Slow 3G) logueado como Admin: navegar a `/pacientes` y `/profesionales` muestra `RouteFallback` en el área de contenido con navbar/sidebar visibles, sin flash de pantalla en blanco.
- [ ] 3.4 Verificar manualmente logueado como Profesional que el menú no ofrece "Pacientes"/"Profesionales" y que navegar directo por URL a `/pacientes` sigue resultando en `/403` (comportamiento de `RequireRole` sin cambios).
- [ ] 3.5 `pnpm test` en verde (sin tests nuevos esperados — este change no agrega lógica testeable más allá del routing).

## 4. Cierre

- [ ] 4.1 Correr `/opsx:archive lazy-load-frontend-routes` cuando esté todo verificado, y dejar una nota en `frontend-architecture.md`/Engram si el patrón (`React.lazy` + `Suspense` por ruta) se vuelve convención para futuras rutas.
