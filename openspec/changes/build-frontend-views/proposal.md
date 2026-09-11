## Why

El frontend tiene el scaffold armado (`frontend/src/{app,core,shared,features}`, httpClient con cola 401, AuthContext con hidratación optimista, guards, hooks `use*Query`, `machine.ts` con test) pero las pantallas son stubs: `TurnoDialog`, `FiltrosTurnos` y `TurnoDetailDrawer` devuelven `null`, `shadcn/ui` no está inicializado y `styles/index.css` todavía tiene la paleta neutra por defecto. Sin vistas no hay entregable demostrable para la prueba técnica (deadline sáb 12/09/2026 22:30).

## What Changes

- **Sistema visual propio**: reemplazar el bloque `@theme` neutro de `styles/index.css` por tokens **rosa pastel + blanco** (canvas rosado tenue, superficies blancas, rosa profundo `#C25C82` como acción/nav/foco, texto ciruela `#3A2A31`), tipografía **Manrope** (webfont con fallback `system-ui`), radios y una única sombra de overlay con tinte rosa. Inicializar `shadcn/ui` con estos tokens como base.
- **Íconos con `lucide-react` en toda la UI**: navegación, botones, badges de estado, estados vacíos, estados de error y toasts. **Prohibido el emoji** en cualquier texto visible.
- **Shell de aplicación**: `MainLayout` con sidebar/topbar responsive, `Navbar` con nombre + badge de rol + logout, menú condicionado por rol (Profesional solo "Mis turnos"), pantalla de carga a pantalla completa mientras `auth.status === 'loading'` (sin flash de `/login`).
- **Vista de login**: `LoginPage` en `/login` con form + zod, toggle de contraseña, error 401 inline, redirección por rol a `/turnos`.
- **Vistas de turnos** (`/turnos`, Admin y Profesional): `PageHeader` con título contextual, `FiltrosTurnos` sincronizados a la query string, `DataTable` paginada con badge de estado, `TurnoDetailDrawer` (detalle + datos del paciente + `EstadoControl` con solo las transiciones legales de `machine.ts` según rol), `TurnoDialog` crear/editar (solo Admin) y `ConfirmDialog` de cancelación.
- **Vistas de maestros** (`/pacientes`, `/profesionales`, solo Admin): `DataTable` con búsqueda debounced, `Dialog` crear/editar validado por schema zod, `ConfirmDialog` de eliminación con manejo explícito del **409 "tiene turnos activos"**.
- **Feedback y errores transversales**: `ErrorBoundary` raíz, `Toaster` (sonner) con ícono lucide por tipo, toast genérico para 500 / error de red, páginas `/403` (`ForbiddenPage`) y `*` (`NotFoundPage`) dentro del shell.
- **No** se agregan librerías de data-fetching (TanStack Query queda como mejora futura), **no** hay dark mode, **no** se implementa el rol Paciente (vive en `add-patient-self-service`, diferido a fase 2).

## Capabilities

### New Capabilities

- `frontend-visual-system`: tokens de color rosa pastel + blanco en `styles/index.css`, tipografía Manrope, escala tipográfica, radios y sombra de overlay, badges de estado del turno (Pendiente/Confirmado/Cancelado/Atendido), regla de íconos lucide y prohibición de emojis, inicialización de `shadcn/ui` sobre estos tokens.
- `frontend-app-shell`: `MainLayout` + `Navbar` responsive, menú condicionado por rol (solo UX), estado de carga de sesión sin flash de login, `ErrorBoundary` y `Toaster` globales en `providers.tsx`, toast genérico de 500 / error de red, páginas `/403` y `*`.
- `frontend-auth-views`: `LoginPage` (form email + contraseña con zod, toggle de visibilidad, estado de envío), error 401 inline "Email o contraseña incorrectos", redirección por rol respetando `from`, acción de logout desde el `Navbar`.
- `frontend-turnos-views`: `TurnosPage` con título contextual por rol, `FiltrosTurnos` (rango de fechas, estado, y profesional/paciente solo Admin) sincronizados a la query string, `DataTable` paginada, `TurnoDetailDrawer` con datos del paciente y `EstadoControl` (transiciones legales según estado y rol vía `machine.ts`), `TurnoDialog` crear/editar (solo Admin) con manejo de 400/404/409, `ConfirmDialog` de cancelación que libera el slot.
- `frontend-maestros-views`: `PacientesPage` y `ProfesionalesPage` (solo Admin) con `DataTable` + búsqueda debounced, `PacienteDialog` / `ProfesionalDialog` crear/editar validados por schema zod con errores por campo desde ProblemDetails, `ConfirmDialog` de eliminación con manejo del 409 "tiene turnos activos" (toast con `detail`).

### Modified Capabilities

<!-- Ninguna: openspec/specs/ está vacío; todas las capacidades son nuevas. -->

## Impact

- **Código**: `frontend/src/styles/index.css` (tokens), `frontend/src/app/providers.tsx` (ErrorBoundary + Toaster), `frontend/src/core/layouts/*` (MainLayout, Navbar, InitialsBadge), `frontend/src/features/auth/pages/LoginPage.tsx`, `frontend/src/features/turnos/*` (pages/components: TurnosPage, FiltrosTurnos, TurnoDetailDrawer, TurnoDialog, EstadoControl), `frontend/src/features/pacientes/*` y `frontend/src/features/profesionales/*` (pages + Dialog), `frontend/src/pages/{ForbiddenPage,NotFoundPage}.tsx`, `frontend/src/shared/components/*` (DataTable, ConfirmDialog, PageHeader, EmptyState, FieldError) y `frontend/src/shared/components/ui/*` (generados por shadcn).
- **Dependencias**: `lucide-react`, familia `Manrope` (Google Fonts o `@fontsource/manrope`), componentes de `shadcn/ui` (`button`, `input`, `select`, `dialog`, `table`, `badge`, `sonner`, `form`, `dropdown-menu`, `calendar`, `popover`, `command`, `textarea`). `sonner` para toasts.
- **Contrato**: consume las rutas del `README.md` sin cambios; no toca el backend.
- **Docs**: actualizar `frontend.md` (marcar tareas §1.3, §3–§9), sección "Frontend" del `README.md` y `PROMPTS.md` / `docs/uso-de-ia.md`.
- **Estimación**: ~18–22 h (sistema visual + shadcn init ~4 h; shell + feedback ~3 h; login ~2 h; turnos ~7 h; maestros ~4 h; pulido responsive/a11y ~2 h). Entra en el presupuesto de 48 h.

## Non-goals

- Rol Paciente / autoservicio de turnos (cubierto por `add-patient-self-service`, fase 2).
- Librería de data-fetching (TanStack Query), cache compartida, dedupe.
- Dark mode / theming múltiple.
- Revertir estados terminales (`Atendido`, `Cancelado`) — requiere auditoría, mejora futura.
- Tests E2E; la cobertura de front se limita a `machine.ts`, `useProblemForm`, interceptor 401 y guards (ya previsto en `arquitectura-frontend.md §3.8`).
