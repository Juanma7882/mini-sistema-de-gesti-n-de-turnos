## Context

`frontend/src/app/router.tsx` arma un único `createBrowserRouter` con imports estáticos de las 6 páginas de la app (`LoginPage`, `TurnosPage`, `PacientesPage`, `ProfesionalesPage`, `ForbiddenPage`, `NotFoundPage`). `App.tsx` monta `<RouterProvider router={router} />` sin `fallbackElement`. `PacientesPage`/`ProfesionalesPage` están además bajo `RequireRole roles={['Admin']}`, así que un usuario Profesional nunca las renderiza pero hoy sí las descarga y parsea en el bundle inicial, junto con sus diálogos (`PacienteDialog`, `ProfesionalDialog`) y las dependencias que arrastran (react-hook-form + zod resolvers de esos schemas).

`shared/components/FullScreenSpinner.tsx` ya existe como fallback visual, pero está pensado para estados *pre-shell* (hidratación de sesión, antes de que exista `MainLayout`): usa `min-h-dvh` y no deja rastro del navbar/sidebar. Las 4 rutas candidatas a lazy (`PacientesPage`, `ProfesionalesPage`, `ForbiddenPage`, `NotFoundPage`) están todas anidadas **dentro** de `MainLayout` en el árbol de rutas, así que reusar `FullScreenSpinner` tal cual taparía el navbar/sidebar durante la carga del chunk — una regresión visual respecto a dejar el shell montado y mostrar el loading solo en el área de contenido.

## Goals / Non-Goals

**Goals:**
- Que el bundle inicial (chunk cargado en `/login` y en `/turnos`) no incluya el código de `PacientesPage`/`ProfesionalesPage` ni el de `ForbiddenPage`/`NotFoundPage`.
- Que el fallback de carga no oculte el shell (navbar/sidebar) ya montado, para no reintroducir el tipo de "flash" que el sistema visual (`build-frontend-views`) ya evitó a propósito en la carga de sesión.
- Mantener los guards existentes (`RequireAuth`, `RequireRole`) evaluándose *antes* de que se dispare la descarga del chunk — no cambia el modelo de autorización, solo el momento en que se baja el JS.

**Non-Goals:**
- No lazy-cargar `LoginPage` ni `TurnosPage` (entrada y home de todo usuario; no hay ganancia, solo latencia extra).
- No lazy-cargar diálogos/drawers (`TurnoDialog`, `PacienteDialog`, `ProfesionalDialog`, `TurnoDetailDrawer`) en este change — se listan en Open Questions como candidato para una iteración futura, no bloquean esta entrega.
- No agregar prefetch en hover/viewport ni tocar `vite.config.ts` (manualChunks) — `import()` dinámico ya le alcanza a Rollup para splittear por defecto.
- No migrar de `createBrowserRouter` a otra API de routing.

## Decisions

**1. `React.lazy()` + `<Suspense>` alrededor del `element`, no la API `route.lazy` de react-router.**
React Router 6.4+/7 soporta un campo `lazy` por ruta pensado para data routers, pero su contrato de "loading" se integra con `useNavigation()`/`HydrateFallback` en vez de `Suspense` puro, y para el *primer* load de una ruta lazy en un data router sin SSR conviene configurar `HydrateFallback` o `fallbackElement` — ninguno de los dos está hoy en `RouterProvider`. Meter esa pieza para un cambio de ~1h no se justifica: `React.lazy` + `Suspense` es el mecanismo estándar de React, ya lo entiende cualquiera que lea el código, y no requiere tocar `App.tsx`/`RouterProvider`. Se reconsidera si en el futuro se agregan `loader`s (ahí sí conviene migrar a `route.lazy` para paralelizar fetch de datos + chunk).

**2. Un `<Suspense>` por ruta lazy, no uno compartido envolviendo todo `MainLayout`.**
Envolver cada `element: <Suspense fallback={...}><PacientesPage/></Suspense>` en vez de un único boundary en el layout limita el "blast radius": si en el futuro se agrega un lazy anidado dentro de `TurnosPage` (fuera de este change), no cae bajo el mismo boundary que `PacientesPage` y no hay interferencia entre pantallas no relacionadas.

**3. Fallback visual nuevo y acotado al área de contenido, no `FullScreenSpinner` reusado tal cual.**
Se agrega un componente chico (p.ej. `shared/components/RouteFallback.tsx`) con el mismo lenguaje visual que `FullScreenSpinner` (ícono `Loader2` girando, `text-primary`, `role="status"`) pero sin `min-h-dvh` — ocupa el área donde va el `<Outlet/>`, dejando navbar/sidebar visibles. Evita el regreso del "flash de pantalla en blanco" que el shell ya resolvió para el login/hidratación.

## Risks / Trade-offs

- [Riesgo] Descarga de chunk lenta en conexión mala → usuario ve el spinner de contenido más tiempo del esperado. Mitigación: los chunks de `PacientesPage`/`ProfesionalesPage` son pequeños (páginas CRUD simples, sin librerías pesadas nuevas); no se justifica agregar retry/timeout UI para el alcance de esta entrega.
- [Riesgo] Un `import()` con typo de path o mal `default export` rompe el build recién en tiempo de build/runtime, no en el editor si el chequeo de tipos no cubre bien `lazy()`. Mitigación: `pnpm typecheck && pnpm build` como paso de verificación en `tasks.md`, igual que ya exige `build-frontend-views §8.4`.
- [Trade-off] Se acepta no lazy-cargar los diálogos pesados (mayor ganancia de KB, pero mayor superficie de cambio y riesgo de romper flujos de alta/edición ya probados) a cambio de terminar el change dentro del presupuesto de 48 h sin arriesgar funcionalidad crítica del CRUD.

## Migration Plan

Cambio de solo-frontend, sin datos ni contrato de API involucrados:
1. Implementar (ver `tasks.md`).
2. Verificar `pnpm typecheck && pnpm lint && pnpm build && pnpm test`.
3. Verificar manualmente en `pnpm build && pnpm preview` (o inspeccionando `dist/assets/`) que aparecen chunks separados para `PacientesPage`/`ProfesionalesPage`/`ForbiddenPage`/`NotFoundPage`.
4. Rollback trivial si algo falla: revertir el commit (vuelve a imports estáticos), no hay estado persistente que limpiar.

## Open Questions

- ¿Vale la pena, en una iteración futura (fuera de esta entrega), extender el lazy loading a `TurnoDialog`/`PacienteDialog`/`ProfesionalDialog`/`TurnoDetailDrawer`? Son los componentes más pesados (react-hook-form + zod + schemas) pero también los más usados por Admin en cada sesión, así que el ahorro real de "primera carga" es menor que el de las rutas — candidato a revisar después de la entrega, no antes.
