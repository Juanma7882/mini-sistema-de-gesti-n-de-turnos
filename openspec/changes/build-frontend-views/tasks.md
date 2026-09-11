## 1. Sistema visual base

- [x] 1.1 Instalar `@fontsource/manrope` (pesos 400/500/600/700) e importarlo en `src/main.tsx` o `styles/index.css`; definir la familia y `font-display: swap`.
- [x] 1.2 Reescribir el bloque `@theme` de `src/styles/index.css` con la paleta rosa pastel + blanco (`--color-canvas`, `--color-surface`, `--color-primary`, `--color-primary-strong`, `--color-primary-tint`, `--color-foreground`, `--color-muted-foreground`, `--color-border`, `--color-ring`, `--color-destructive`); actualizar `body` para usar `--color-canvas` / `--color-foreground`.
- [ ] 1.3 Definir escala tipográfica formal (título login, título página, título diálogo, cuerpo 14px, label/meta, header de tabla) — hoy los tamaños se eligen ad-hoc por componente, no hay una escala documentada; `tabular-nums` sí se usa donde corresponde (Inicio de turnos).
- [ ] 1.4 Radios: `10px` (cards/inputs/diálogos) y `6px` (badges) ya se usan consistentemente; falta unificar `8px` en botones (hoy varían entre `rounded-lg`/`rounded-md`) y no hay un único token de sombra de overlay (cada overlay define su propio `shadow-[...]` inline).
- [~] 1.5/1.6 (cerrada por decisión, no se hace) Se intentó `pnpm dlx shadcn@latest init`: el CLI nuevo (presets Nova/Vega/..., elección Base UI/Radix/Aria) tira "Could not load the workspace config" incluso con alias `@/` temporal configurado; la versión pineada sugerida por el propio error también fallaba en runs no interactivos. Decisión con el usuario: **sin shadcn/Radix** — todo hand-rolled con Tailwind puro (`Dialog`, `ConfirmDialog`, `DataTable`, `FullScreenSpinner`, etc. en `shared/components/`), mismo patrón ya usado en el resto del proyecto. `paths`/`resolve.alias` temporales revertidos, cero rastro del intento fallido.
- [x] 1.7 `pnpm typecheck && pnpm lint && pnpm build && pnpm test` en verde.

## 2. Componentes compartidos y de estado

- [x] 2.1 `shared/components/PageHeader.tsx`: título + slot de acción (botón primario opcional).
- [ ] 2.2 (parcial) `shared/components/DataTable.tsx`: columnas configurables ✓, hover de fila `--color-primary-tint` ✓, fila activable por teclado (Enter) ✓, loading con spinner (no shimmer/skeleton de filas) — falta el pie de paginación (`‹ ›`, "Página X de Y"): todas las páginas piden `page:1` fijo, no hay UI para cambiar de página.
- [x] 2.3 `shared/components/EmptyState.tsx`: wrapper simple; cada caller compone ícono lucide + texto + acción opcional (sin emoji) — así se usa en Turnos/Pacientes/Profesionales.
- [x] 2.4 `shared/components/FieldError.tsx`: ícono `alert-circle` 14px + mensaje; integrado con `useProblemForm`.
- [x] 2.5 `shared/components/ConfirmDialog.tsx`: ícono en círculo rosa-tint, título (vía `Dialog`), cuerpo, botón destructivo + botón de cancelar. Nuevo `shared/components/Dialog.tsx` genérico (overlay + Escape) reusado por `ConfirmDialog` y los tres diálogos de formulario.
- [x] 2.6 `EstadoBadge` (`features/turnos/components/EstadoBadge.tsx`) + mapa único `EstadoTurno -> { fill, text, icon, label }` (Pendiente ámbar/`clock`, Confirmado rosa/`check-circle-2`, Atendido salvia/`clipboard-check`, Cancelado gris malva/`x-circle`); consumido por la tabla de turnos y el drawer de detalle.

## 3. Shell, feedback y páginas de error

- [x] 3.1 `core/layouts/InitialsBadge.tsx`: iniciales sobre fondo rosa-tint, radio `10px`.
- [x] 3.2 `core/layouts/Navbar.tsx`: nombre de usuario, `InitialsBadge`, badge de rol, acción logout (`log-out`) que llama `POST /auth/logout` + limpia sesión + navega a `/login`.
- [x] 3.3 `core/layouts/MainLayout.tsx`: sidebar 240px en `>=768px`, topbar + panel deslizable (`menu`) en mobile; contenido con canvas, `max-width` ~1200px y padding 24–32px; `<Outlet/>`.
- [x] 3.4 Menú condicionado por rol: Profesional solo "Mis turnos"; Admin "Turnos"/"Pacientes"/"Profesionales"; ítem activo con fondo tint + barra 3px + texto primario.
- [x] 3.5 Pantalla de carga full-screen con `loader-2` mientras `auth.status === 'loading'`; sin render de layout ni de `/login`.
- [x] 3.6 `app/providers.tsx`: `ErrorBoundary` raíz (pantalla `alert-octagon` + "Recargar") y `Toaster` (sonner) global con ícono lucide por tipo (éxito salvia, error rojo cálido).
- [x] 3.7 Toast genérico para 500 / error de red desde el interceptor del `httpClient` (`wifi-off`), sin exponer detalle técnico.
- [x] 3.8 `pages/ForbiddenPage.tsx` (`shield-x`, "No tenés permiso para ver esta página.", enlace "Ir a turnos") y `pages/NotFoundPage.tsx` (`compass`, "No encontramos esta página.", enlace "Volver al inicio"), ambas dentro del shell.
- [ ] 3.9 (parcial) Anillo de foco 2px `--color-ring` global ya está (regla `:focus-visible` en `styles/index.css`); falta el respeto de `prefers-reduced-motion` en shimmer y transiciones de overlay, que se hace cuando existan esos componentes (skeleton de `DataTable` §2.2, diálogos/drawer §6–7).

## 4. Login

- [x] 4.1 Afinar `features/auth/pages/LoginPage.tsx` al sistema visual: tarjeta ~380px centrada sobre canvas, marca `heart-pulse` en tile rosa-tint, encabezado "Ingresá a tu cuenta".
- [x] 4.2 Campos con `react-hook-form` + `zodResolver(loginSchema)`: Email (`mail`), Contraseña (`lock`) con toggle `eye`/`eye-off`.
- [x] 4.3 Envío: `POST /auth/login`, guardar token en memoria + snapshot `{ nombre, role }`; botón con `loader-2` + "Ingresando" + deshabilitado.
- [x] 4.4 Redirección por rol respetando `from`; 401 → alerta inline "Email o contraseña incorrectos." (`alert-circle`); 400 → `FieldError` por campo.
- [x] 4.5 Responsive `<420px`: tarjeta a ancho completo con gutter ~20px, sin scroll horizontal.
- [x] 4.6 (agregado) `RedirectIfAuthenticated`: con sesión activa, `/login` redirige a `/turnos` en vez de mostrar el formulario; mientras `status === 'loading'` muestra el mismo `FullScreenSpinner` que `RequireAuth` (sin flash de login ni de shell). Login queda completo end-to-end: nadie entra a la app sin pasar por `/login`, y nadie ve `/login` ya autenticado.

## 5. Turnos — listado y detalle

- [x] 5.1 `features/turnos/pages/TurnosPage.tsx`: `PageHeader` con título contextual por rol; botón "Nuevo turno" (`plus`) solo Admin; no enviar `profesionalId` propio.
- [~] 5.2 (parcial) `FiltrosTurnos.tsx`: rango de fechas con `<input type="date">` nativo (no `popover`+`calendar`, no se instaló shadcn — ver §1.5/1.6), estado (`select`, sin swatch de color en la opción), y solo Admin: profesional (`select`); "Limpiar filtros" (`x`) visible solo con filtros activos. **Falta el filtro de paciente por búsqueda**: `TurnosQuery` del backend no tiene un parámetro de texto libre, solo `pacienteId` — no se agregó un combobox de paciente solo para filtrar.
- [x] 5.3 Filtros sincronizados a la query string (`?desde&hasta&estado&profesionalId`) vía `useSearchParams` de react-router (no se usó `queryString.ts`, que sigue reservado para armar querystrings de request); rehidratan al recargar.
- [x] 5.4 Tabla de turnos sobre `DataTable`: columnas Paciente, Profesional (oculta para rol Profesional, ya que siempre sería su propio nombre), Inicio (`formatInicio`, `tabular-nums`), Estado (`EstadoBadge`), Notas (truncadas con `line-clamp-1`), `chevron-right`.
- [~] 5.5 (parcial) Loading con spinner (no skeleton de 6–8 filas); `EmptyState` `calendar-search` "No hay turnos para estos filtros." + "Limpiar filtros"; vacío total → "Todavía no hay turnos." + "Nuevo turno" (solo Admin); error no-500 → panel `alert-triangle` + "Reintentar" (ver `TurnosPage`, condiciona `error.status < 500 && !== 0`; 500/red ya van por el toast global de `httpClient`).
- [x] 5.6 `TurnoDetailDrawer.tsx`: panel derecho ~420px (`max-w-105`, full-width en mobile), refleja `?turno=<id>` (recargable); secciones Turno / Paciente (`phone` con `tel:`, `shield`) / Profesional (`stethoscope`) separadas por hairline; estado 404 → `search-x` "Este turno ya no está disponible." + "Volver al listado".
- [x] 5.7 Abrir el drawer desde click o Enter en la fila (`DataTable` soporta `tabIndex`+`onKeyDown` cuando hay `onRowClick`).

## 6. Turnos — acciones

- [x] 6.1 `EstadoControl.tsx`: renderiza solo transiciones legales de `machine.ts` para (estado, rol); botones Confirmar `check`, Marcar atendido `clipboard-check`, Cancelar `x-circle`; sin acciones → "Sin acciones disponibles".
- [x] 6.2 `PATCH /turnos/{id}/estado`; 409 → alerta inline en el drawer con `detail`, sin cambiar el estado en la UI.
- [~] 6.3 (simplificado) `TurnoDialog.tsx` (solo Admin, abierto desde "Nuevo turno" o desde "Editar turno" en el drawer): buscador de texto + `select` de paciente (sin `command`/`popover`), `select` de profesional, `<input type="date">` + `<input type="time">` en vez de `calendar` (se combinan en el string local naïve que espera el backend), `textarea` de notas.
- [x] 6.4 Crear (`POST /turnos`) y editar (`PUT /turnos/{id}`): botón con spinner + "Guardando"; éxito → cerrar + toast + refetch.
- [x] 6.5 Errores: 409 slot ocupado → alerta `calendar-x` con `detail`; 400 → `useProblemForm` por campo; 404 → "El paciente o profesional seleccionado ya no existe."
- [x] 6.6 Cancelación: `ConfirmDialog` ("El horario quedará libre para otro paciente.", botón "Sí, cancelar") → `PATCH estado="Cancelado"` → toast "Turno cancelado" + refetch (libera slot). Antes el botón "Cancelar" del `EstadoControl` disparaba el cambio directo; ahora el drawer intercepta la transición `Cancelado` y pide confirmación primero.

## 7. Maestros — pacientes y profesionales

- [~] 7.1 (simplificado) `PacientesPage.tsx`: ruta ya bajo `RequireRole` Admin (`router.tsx`); `PageHeader` + "Nuevo paciente"; búsqueda `search` con debounce 300ms; `DataTable` desde `GET /pacientes?search=&page=&pageSize=` (Nombre, Apellido, Teléfono, Obra social — sin columna "Alta") + columna "Acciones" con dos íconos inline (`pencil`/`trash-2`) en vez de un menú `more-horizontal` (no hay `dropdown-menu` de shadcn).
- [x] 7.2 Estados vacíos: búsqueda sin resultados → `user-search` "No encontramos pacientes con ese texto."; sin datos → `users` "Todavía no hay pacientes." + "Nuevo paciente".
- [x] 7.3 `PacienteDialog.tsx`: campos Nombre, Apellido, Teléfono, Obra social con `pacienteSchema`; crear `POST` / editar `PUT`; 400 → errores por campo; éxito → cerrar + toast + refetch.
- [x] 7.4 Eliminar paciente: `ConfirmDialog` (`trash-2`, "No podrás eliminarlo si tiene turnos activos.") → `DELETE`; éxito → toast "Paciente eliminado" + refetch; 409 → toast destructivo "Este paciente tiene turnos activos. Cancelá o reasigná esos turnos primero.".
- [~] 7.5 (simplificado, mismo criterio que 7.1) `ProfesionalesPage.tsx`: misma estructura; columnas Nombre, Apellido, Especialidad (sin "Alta"); vacío con `stethoscope`.
- [x] 7.6 `ProfesionalDialog.tsx`: campos Nombre, Apellido, Especialidad con `profesionalSchema`; crear/editar + errores por campo.
- [x] 7.7 Eliminar profesional: `ConfirmDialog` + `DELETE`; 409 → toast "Este profesional tiene turnos activos. Cancelá o reasigná esos turnos primero.".

## 8. Pulido, verificación y documentación

- [ ] 8.1 Revisar skeletons y estados vacíos en todas las tablas; responsive con scroll horizontal de tabla en mobile; foco/orden de tabulación en diálogos y drawer.
- [ ] 8.2 Grep anti-emoji sobre `frontend/src` (JSX y strings visibles): cero coincidencias.
- [ ] 8.3 Contraste AA de texto sobre `--color-primary`, badges y `muted-foreground`; capturas del shell y de `/turnos` para validar que no lee como "spa".
- [ ] 8.4 `pnpm typecheck && pnpm lint && pnpm build && pnpm test` en verde; tests de `machine.ts` intactos.
- [ ] 8.5 Marcar tareas cumplidas en `frontend.md` (§1.3, §3–§9) y actualizar `arquitectura-frontend.md` si cambió algo del scaffold.
- [ ] 8.6 Agregar la sección "Frontend" de "Cómo correr localmente" al `README.md` y volcar los prompts de IA usados a `PROMPTS.md` / `docs/uso-de-ia.md`.
