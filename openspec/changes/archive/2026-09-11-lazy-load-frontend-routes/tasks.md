## 1. Fallback visual

- [x] 1.1 Crear `frontend/src/shared/components/RouteFallback.tsx`: mismo lenguaje visual que `FullScreenSpinner` (`Loader2` de lucide, `animate-spin`, `text-primary`, `role="status" aria-label="Cargando"`) pero sin `min-h-dvh` — pensado para ocupar el área de contenido dentro de `MainLayout`, no el viewport completo.

## 2. Code-splitting en el router

- [x] 2.1 En `frontend/src/app/router.tsx`, reemplazar el import estático de `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage` y `NotFoundPage` por `React.lazy(() => import('...'))`.
- [x] 2.2 Envolver cada uno de esos 4 `element` con `<Suspense fallback={<RouteFallback />}>...</Suspense>` (un `Suspense` por ruta, no uno compartido — ver `design.md` Decisión 2). Dejar `LoginPage` y `TurnosPage` con import estático, sin Suspense.
- [x] 2.3 Confirmar que `RequireRole`/`RequireAuth` siguen envolviendo las rutas lazy en el mismo punto del árbol que antes (el `Suspense` va *dentro* del guard, no en vez de él).

## 3. Verificación

- [x] 3.1 `pnpm typecheck && pnpm lint` en verde. (Se agregó `eslint-disable react-refresh/only-export-components` puntual en `router.tsx`: el archivo es config de rutas, no un módulo de componentes de UI, así que fast-refresh no aplica.)
- [x] 3.2 `pnpm build`; inspeccionar `dist/assets/` (o el resumen que imprime Vite) y confirmar que aparecen chunks separados para `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage` y `NotFoundPage`, distintos del chunk principal. Confirmado: `PacientesPage-*.js` (6.39 kB), `ProfesionalesPage-*.js` (6.52 kB), `ForbiddenPage-*.js` (1.07 kB), `NotFoundPage-*.js` (0.90 kB), fuera de `index-*.js`.
- [x] 3.3 Verificado con Playwright (pedido explícito del usuario) contra `pnpm dev` (localhost:5173, con el chunk de `PacientesPage`/`ProfesionalesPage` demorado artificialmente vía `page.route` para forzar la ventana de carga): logueado como Admin, `RouteFallback` (spinner `role="status"`) aparece en el área de contenido con `<aside>` (sidebar) visible, sin flash de pantalla en blanco. Confirmado por captura.
- [x] 3.4 Verificado con Playwright: logueado como `dra.gomez@clinica.test` (rol Profesional), el menú solo muestra "Mis turnos" (0 links a "Pacientes"/"Profesionales"); navegar directo a `/pacientes` por URL redirige a `/403` y renderiza `ForbiddenPage` ("No tenés permiso para ver esta página."). Confirmado por captura.
- [x] 3.5 `pnpm test` en verde (sin tests nuevos esperados — este change no agrega lógica testeable más allá del routing). 3 tests pasan.

## 4. Cierre

- [ ] 4.1 Correr `/opsx:archive lazy-load-frontend-routes` cuando esté todo verificado, y dejar una nota en `frontend-architecture.md`/Engram si el patrón (`React.lazy` + `Suspense` por ruta) se vuelve convención para futuras rutas.
