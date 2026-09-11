# Frontend — Lista de tareas

Alcance: contrato del `README.md` (2 roles: Administrador y Profesional).
Stack: React + TypeScript (Vite) · Tailwind CSS v4 (componentes propios, sin
shadcn/ui — ver `arquitectura-frontend.md` §3.7) · React Router · SignalR.
Convención: imports relativos (ver commit `e3b43f6`).

> **Estado (2026-09-11):** §1–§9 completas, frontend funcional para las tres
> entidades (turnos, pacientes, profesionales) en ambos roles. Pendiente real:
> §10 (deploy a Vercel, manual, necesita la cuenta) y §11.2 (prompts de IA del
> frontend en `uso-de-ia.md`, todavía no volcados).

Orden por camino crítico: **arranca y loguea** antes que pantallas; pantallas de
Admin antes que las de Profesional (reusan componentes). Tareas de máx. ~2 h. El
rol **Paciente** NO entra acá (ver `openspec/changes/add-patient-self-service/tasks.md`).

---

## 1. Scaffolding

> Hecho con **pnpm** (no npm). Toolchain: Vite 8 · React 19 · TS 6 · Tailwind v4
> (`@tailwindcss/vite`, CSS-first) · Vitest. `pnpm typecheck`, `pnpm lint`,
> `pnpm build` y `pnpm test` pasan en verde.

- [x] 1.1 `pnpm create vite@latest frontend --template react-ts`; boilerplate limpiado.
- [x] 1.2 Tailwind CSS v4: plugin `@tailwindcss/vite`, `@import 'tailwindcss'` + `@theme` en `styles/index.css`.
- [x] 1.3 **Desviación del plan**: no se inicializó `shadcn/ui`. Se construyeron a mano en `shared/components/` los componentes que se usan (`Dialog`, `DataTable`, `ConfirmDialog`, `PageHeader`, `FieldError`, `EmptyState`, `RouteFallback`) sobre Tailwind directo, con `sonner` para toasts y `lucide-react` para íconos. `src/shared/components/ui/` quedó vacía (solo su `README.md` explicando el plan original) — candidata a borrarse. Detalle en `arquitectura-frontend.md` §3.7.
- [x] 1.4 React Router: `createBrowserRouter`, layout raíz, rutas `/login`, `/turnos`, `/pacientes`, `/profesionales`, `/403`, `*` (404).
- [x] 1.5 Estructura de carpetas según `arquitectura-frontend.md` §2 (`src/{app,core,shared,features,pages,styles,tests}`); `.env.example` con `VITE_API_URL`.
- [x] 1.6 ESLint (flat config) + Prettier + script `typecheck`; `pnpm dev` y `pnpm build` verificados.

## 2. Cliente HTTP y sesión

- [x] 2.1 Tipos compartidos: no en `src/types` como se había pensado, sino `shared/types/api.ts` (`PagedResult<T>`, `ProblemDetails`, `EstadoTurno`) + `types.ts` por feature (`TurnoDto`, `PacienteResumen`, `ProfesionalResumen`) — sigue la convención real de `arquitectura-frontend.md` §3.5.
- [x] 2.2 Wrapper `fetch` en `core/api/httpClient.ts`: `baseURL = VITE_API_URL`, `credentials: 'include'`, header `Authorization: Bearer` desde token en memoria (`ApiError` tipado, nunca se propaga un `Response` crudo).
- [x] 2.3 Store de auth en memoria (`AuthContext`, `accessToken`/`user`/`status`) + snapshot no sensible `{ nombre, role }` en `localStorage` (`core/auth/session.ts`) solo para pintar el menú.
- [x] 2.4 Interceptor 401 en `httpClient`: un único `POST /auth/refresh` en vuelo (`refreshInFlight` compartido), reintenta el original; si refresh falla → `onAuthLost()` limpia sesión y va a `/login`.
- [x] 2.5 Hidratación optimista al bootear (`AuthProvider`): dispara `refresh`; 200 → entra directo, 401 → `/login`. Sin flash de login (estado `loading`, `RequireAuth` no decide nada mientras tanto).
- [x] 2.6 `parseProblemDetails` en `core/api/problemDetails.ts` → `{ title, detail, errors }` normalizado.

## 3. Login y guards

- [x] 3.1 Página `/login` (`LoginPage.tsx`): form email + password con `react-hook-form` + `zodResolver` (no shadcn `form`, ver §1.3), llama `POST /auth/login`, guarda token/user, redirige según `role`.
- [x] 3.2 Manejo de error 401 inline ("credenciales inválidas"); estado de submit.
- [x] 3.3 `<RequireAuth>`: sin sesión → `/login` (guardando `from`).
- [x] 3.4 `<RequireRole roles={[...]}>`: rol incorrecto → `/403`.
- [x] 3.5 Logout: `POST /auth/logout`, limpia memoria + `localStorage` + desconecta SignalR (`turnosHub.disconnect()`), va a `/login`.

## 4. Layout y navegación

- [x] 4.1 Shell con sidebar/topbar (`MainLayout` + `Navbar`): nombre de usuario + badge de rol (`InitialsBadge`) + botón logout.
- [x] 4.2 Menú condicionado por `role`: Profesional ve solo "Mis turnos"; Admin ve Turnos / Pacientes / Profesionales. (Gating solo UX.)
- [x] 4.3 Componentes transversales: `PageHeader`, `DataTable` (paginación + estado vacío + loading), `ConfirmDialog`, `Toaster` (sonner, en `app/providers.tsx`), `FieldError`.
- [x] 4.4 Hook `useProblemForm` (`shared/hooks/`) para volcar `errors` de ProblemDetails a los campos del form.

## 5. Turnos — listado (Admin y Profesional)

- [x] 5.1 Página `/turnos` (`TurnosPage.tsx`): `GET /turnos` con paginación; columnas paciente, profesional, inicio, estado (`EstadoBadge`), notas. Se refresca también por SignalR (`turnoCambiado`, ver `arquitectura-frontend.md` §1.1).
- [x] 5.2 Barra de filtros (`FiltrosTurnos.tsx`): rango de fechas, estado, y (solo Admin) profesional y paciente, sincronizados a la query string (`shared/lib/queryString.ts`).
- [x] 5.3 Título contextual: Admin "Turnos", Profesional "Mis turnos" (el server ya filtra; no se manda `profesionalId`).
- [x] 5.4 Fila → `TurnoDetailDrawer` con datos del paciente y del turno.

## 6. Turnos — acciones

- [x] 6.1 "Nuevo turno" (solo Admin): `TurnoDialog` con buscador de paciente, select de profesional, fecha + hora → `POST /turnos`.
- [x] 6.2 Manejo de 409 "slot ocupado" inline en el form; 404 paciente/profesional; 400 validación por campo (vía `ApiError`/`useProblemForm`).
- [x] 6.3 Editar turno (Admin): mismo `TurnoDialog` para `PUT /turnos/{id}`; revalida 409.
- [x] 6.4 Cambiar estado: `EstadoControl` muestra solo las transiciones legales (`features/turnos/machine.ts`, espejo de la máquina de estados del backend) → `PATCH /turnos/{id}/estado`.
- [x] 6.5 Cancelar turno = `PATCH estado = "Cancelado"` con `ConfirmDialog`; refresca el listado (libera el slot).
- [x] 6.6 Profesional: `machine.ts` devuelve el mismo set de transiciones para ambos roles — la restricción real ("solo sus turnos") la aplica el servidor; el front no repite lógica de scoping por rol acá.

## 7. Pacientes (solo Admin)

- [x] 7.1 Página `/pacientes`: `DataTable` con `GET /pacientes?search=&page=&pageSize=`; búsqueda con `useDebouncedValue`.
- [x] 7.2 Crear / editar en `PacienteDialog` (`nombre`, `apellido`, `telefono`, `obraSocial`) → `POST` / `PUT`; errores por campo desde ProblemDetails.
- [x] 7.3 Eliminar con `ConfirmDialog` → `DELETE`; 409 "tiene turnos activos" con toast.

## 8. Profesionales (solo Admin)

- [x] 8.1 Página `/profesionales`: `DataTable` con `GET /profesionales?search=&page=&pageSize=`.
- [x] 8.2 Crear / editar (`ProfesionalDialog`: `nombre`, `apellido`, `especialidad`) → `POST` / `PUT`.
- [x] 8.3 Eliminar → `DELETE`; manejar 409 igual que pacientes.

## 9. Pulido y errores

- [x] 9.1 Estados de carga (`isLoading` → filas skeleton) y vacíos (`EmptyState`) en `DataTable`.
- [x] 9.2 Toast global para 500 / error de red (`notifyConnectionError` en `httpClient`); 403 → `ForbiddenPage`.
- [x] 9.3 Formato de fecha/hora local consistente (`shared/lib/formatInicio.ts`).
- [x] 9.4 Responsive: `DataTable` con `overflow-x-auto`, `MainLayout` con breakpoints `md`/`sm` (sidebar colapsa a topbar en mobile).
- [x] 9.5 `localStorage` solo tiene `{ nombre, role }` (`core/auth/session.ts`) — verificado, sin token ni datos sensibles.

## 10. Deploy

- [ ] 10.1 Proyecto en Vercel: framework Vite, `VITE_API_URL` apuntando a Railway. **Pendiente** — no hay config de Vercel en el repo todavía (mismo bloqueo que `backend.md` §11.3: necesita las cuentas).
- [ ] 10.2 Verificar que el dominio de Vercel esté en `Cors__AllowedOrigins` del backend y que la cookie `rt` viaje (SameSite/Secure) entre dominios.
- [ ] 10.3 Smoke en prod: login Admin (crear paciente/profesional/turno, cambiar estado), login Profesional (ver solo lo suyo, cambiar estado), F5 sin flash de login, expiración de token → refresh transparente, logout.

## 11. Documentación

- [x] 11.1 Sección "Frontend" en el bloque "Cómo correr localmente" del `README.md`.
- [ ] 11.2 Aportar los prompts de IA usados en el front a `docs/uso-de-ia.md`. **Pendiente real** — ese documento hoy solo cubre el backend.
