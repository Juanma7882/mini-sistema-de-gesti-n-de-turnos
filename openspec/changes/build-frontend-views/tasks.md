## 1. Sistema visual base

- [ ] 1.1 Instalar `@fontsource/manrope` (pesos 400/500/600/700) e importarlo en `src/main.tsx` o `styles/index.css`; definir la familia y `font-display: swap`.
- [ ] 1.2 Reescribir el bloque `@theme` de `src/styles/index.css` con la paleta rosa pastel + blanco (`--color-canvas`, `--color-surface`, `--color-primary`, `--color-primary-strong`, `--color-primary-tint`, `--color-foreground`, `--color-muted-foreground`, `--color-border`, `--color-ring`, `--color-destructive`); actualizar `body` para usar `--color-canvas` / `--color-foreground`.
- [ ] 1.3 Definir escala tipográfica (título login, título página, título diálogo, cuerpo 14px, label/meta, header de tabla) y utilidad de `tabular-nums`; sin `text-transform: uppercase` en labels.
- [ ] 1.4 Definir radios (`10px` cards/inputs/diálogos, `8px` botones, `6px` badges) y una única sombra de overlay con tinte rosa como tokens.
- [ ] 1.5 `pnpm dlx shadcn@latest init` con los tokens propios como base; mapear las variables de shadcn a los tokens en `styles/index.css`.
- [ ] 1.6 Agregar los componentes de shadcn a `src/shared/components/ui/`: `button input select dialog sheet table badge sonner form label dropdown-menu calendar popover command textarea skeleton alert`; reescribir imports `@/` a relativos.
- [ ] 1.7 Verificar `pnpm typecheck && pnpm lint && pnpm build` en verde tras el init.

## 2. Componentes compartidos y de estado

- [ ] 2.1 `shared/components/PageHeader.tsx`: título + slot de acción (botón primario opcional).
- [ ] 2.2 `shared/components/DataTable.tsx`: columnas configurables, hover de fila `--color-primary-tint`, fila activable por teclado, pie de paginación (`‹ ›`, "Página X de Y"), slots de loading (skeleton) y vacío.
- [ ] 2.3 `shared/components/EmptyState.tsx`: ícono lucide + título + acción opcional (sin emoji).
- [ ] 2.4 `shared/components/FieldError.tsx`: ícono `alert-circle` 14px + mensaje; integrado con `useProblemForm`.
- [ ] 2.5 `shared/components/ConfirmDialog.tsx`: ícono en círculo rosa-tint, título, cuerpo, botón destructivo + botón de cancelar.
- [ ] 2.6 `EstadoBadge` + mapa único `EstadoTurno -> { fill, text, icon, label }` (Pendiente ámbar/`clock`, Confirmado rosa/`check`, Atendido salvia/`clipboard-check`, Cancelado gris malva/`x-circle`); consumido por tabla y drawer.

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

- [ ] 5.1 `features/turnos/pages/TurnosPage.tsx`: `PageHeader` con título contextual por rol; botón "Nuevo turno" (`plus`) solo Admin; no enviar `profesionalId` propio.
- [ ] 5.2 `features/turnos/components/FiltrosTurnos.tsx`: rango de fechas (`popover` + `calendar`), estado (`select` con swatch), y solo Admin: profesional (`select`) y paciente (input `search` con debounce 300ms); "Limpiar filtros" (`x`) visible solo con filtros activos.
- [ ] 5.3 Sincronizar filtros con la query string vía `shared/lib/queryString.ts`; rehidratar al recargar.
- [ ] 5.4 Tabla de turnos sobre `DataTable`: columnas Paciente, Profesional, Inicio (`formatInicio`, `tabular-nums`), Estado (`EstadoBadge`), Notas (truncadas), `chevron-right`.
- [ ] 5.5 Estados de la lista: skeleton (6–8 filas) mientras carga; `EmptyState` `calendar-search` "No hay turnos para estos filtros." + "Limpiar filtros"; vacío total Admin → "Todavía no hay turnos." + "Nuevo turno"; error no-500 → panel `alert-triangle` + "Reintentar".
- [ ] 5.6 `features/turnos/components/TurnoDetailDrawer.tsx`: `sheet` derecho ~420px (full-width en mobile), refleja `?turno=<id>`; secciones Turno / Paciente (`phone` con `tel:`, `shield`) / Profesional (`stethoscope`) separadas por hairline; estado 404 → `search-x` "Este turno ya no está disponible." + "Volver al listado".
- [ ] 5.7 Abrir el drawer desde click o Enter en la fila.

## 6. Turnos — acciones

- [ ] 6.1 `features/turnos/components/EstadoControl.tsx`: renderiza solo transiciones legales de `machine.ts` para (estado, rol); botones Confirmar `check`, Marcar atendido `clipboard-check`, Cancelar `x-circle`; sin acciones → "Sin acciones disponibles".
- [ ] 6.2 Ejecutar `PATCH /turnos/{id}/estado`; 409 → alerta inline en el drawer con `detail`, sin cambiar el estado en la UI.
- [ ] 6.3 `features/turnos/components/TurnoDialog.tsx` (solo Admin): combobox de paciente (`command` + `popover`, `GET /pacientes?search=`), `select` de profesional, `calendar` de fecha + selector de hora a minuto (enviar string local naïve), `textarea` de notas.
- [ ] 6.4 `TurnoDialog` crear (`POST /turnos`) y editar (`PUT /turnos/{id}`): botón con spinner + "Guardando"; éxito → cerrar + toast (`check-circle`) + refetch.
- [ ] 6.5 Errores del `TurnoDialog`: 409 slot ocupado → alerta `calendar-x` con `detail` arriba del cuerpo; 400 → `useProblemForm` por campo; 404 → "El paciente o profesional seleccionado ya no existe."
- [ ] 6.6 Cancelación: `ConfirmDialog` ("El horario quedará libre para otro paciente.", botón "Sí, cancelar") → `PATCH estado="Cancelado"` → toast "Turno cancelado" + refetch (libera slot).

## 7. Maestros — pacientes y profesionales

- [ ] 7.1 `features/pacientes/pages/PacientesPage.tsx`: ruta bajo `RequireRole` Admin; `PageHeader` + "Nuevo paciente"; barra de búsqueda `search` con debounce 300ms; `DataTable` desde `GET /pacientes?search=&page=&pageSize=` (Nombre, Apellido, Teléfono, Obra social, Alta) + menú `more-horizontal` (Editar `pencil`, Eliminar `trash-2`).
- [ ] 7.2 Estados vacíos de pacientes: búsqueda sin resultados → `user-search` "No encontramos pacientes con ese texto."; sin datos → `users` "Todavía no hay pacientes." + "Nuevo paciente".
- [ ] 7.3 `features/pacientes/components/PacienteDialog.tsx`: campos Nombre, Apellido, Teléfono (`phone`), Obra social (`shield`) con `pacienteSchema`; crear `POST` / editar `PUT`; 400 → errores por campo; éxito → cerrar + toast + refetch.
- [ ] 7.4 Eliminar paciente: `ConfirmDialog` (`trash-2`, "No podrás eliminarlo si tiene turnos activos.") → `DELETE`; 204 → toast "Paciente eliminado" + refetch; 409 → toast destructivo `alert-triangle` "Este paciente tiene turnos activos. Cancelá o reasigná esos turnos primero.".
- [ ] 7.5 `features/profesionales/pages/ProfesionalesPage.tsx`: misma estructura reusando `DataTable` / búsqueda / menú; columnas Nombre, Apellido, Especialidad, Alta; vacío con `stethoscope`.
- [ ] 7.6 `features/profesionales/components/ProfesionalDialog.tsx`: campos Nombre, Apellido, Especialidad (`stethoscope`) con `profesionalSchema`; crear/editar + errores por campo.
- [ ] 7.7 Eliminar profesional: `ConfirmDialog` + `DELETE`; 409 → toast "Este profesional tiene turnos activos. Cancelá o reasigná esos turnos primero.".

## 8. Pulido, verificación y documentación

- [ ] 8.1 Revisar skeletons y estados vacíos en todas las tablas; responsive con scroll horizontal de tabla en mobile; foco/orden de tabulación en diálogos y drawer.
- [ ] 8.2 Grep anti-emoji sobre `frontend/src` (JSX y strings visibles): cero coincidencias.
- [ ] 8.3 Contraste AA de texto sobre `--color-primary`, badges y `muted-foreground`; capturas del shell y de `/turnos` para validar que no lee como "spa".
- [ ] 8.4 `pnpm typecheck && pnpm lint && pnpm build && pnpm test` en verde; tests de `machine.ts` intactos.
- [ ] 8.5 Marcar tareas cumplidas en `frontend.md` (§1.3, §3–§9) y actualizar `arquitectura-frontend.md` si cambió algo del scaffold.
- [ ] 8.6 Agregar la sección "Frontend" de "Cómo correr localmente" al `README.md` y volcar los prompts de IA usados a `PROMPTS.md` / `docs/uso-de-ia.md`.
