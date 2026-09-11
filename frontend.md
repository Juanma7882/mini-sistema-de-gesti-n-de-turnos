# Frontend — Lista de tareas

Alcance: contrato del `README.md` (2 roles: Administrador y Profesional).
Stack: React + TypeScript (Vite) · Tailwind CSS + shadcn/ui · React Router.
Convención: imports relativos (ver commit `e3b43f6`).

Orden por camino crítico: **arranca y loguea** antes que pantallas; pantallas de
Admin antes que las de Profesional (reusan componentes). Tareas de máx. ~2 h. El
rol **Paciente** NO entra acá (ver `openspec/changes/add-patient-self-service/tasks.md`).

---

## 1. Scaffolding

- [ ] 1.1 `npm create vite@latest frontend -- --template react-ts`; limpiar boilerplate.
- [ ] 1.2 Tailwind CSS: instalar, `tailwind.config`, directivas en `index.css`.
- [ ] 1.3 shadcn/ui: `init` + tema base; añadir componentes que se usan (`button`, `input`, `select`, `dialog`, `table`, `badge`, `sonner/toast`, `form`, `dropdown-menu`, `calendar`, `popover`).
- [ ] 1.4 React Router: `createBrowserRouter`, layout raíz, rutas `/login`, `/turnos`, `/pacientes`, `/profesionales`, `*` (404).
- [ ] 1.5 Estructura de carpetas según `arquitectura-frontend.md` §2 (`src/{app,core,shared,features,pages,styles,tests}`); `.env.example` con `VITE_API_URL`.
- [ ] 1.6 ESLint + Prettier + script `typecheck`; verificar `npm run dev` y `npm run build`.

## 2. Cliente HTTP y sesión

- [ ] 2.1 Tipos compartidos en `src/types`: `PacienteDto`, `ProfesionalDto`, `TurnoDto`, `EstadoTurno`, `PagedResult<T>`, `ProblemDetails`.
- [ ] 2.2 Wrapper `fetch`/axios con `baseURL = VITE_API_URL`, `credentials: 'include'`, header `Authorization: Bearer` desde token en memoria.
- [ ] 2.3 Store de auth en memoria (`accessToken`, `user`) + snapshot no sensible `{ nombre, role }` en `localStorage` solo para pintar el menú.
- [ ] 2.4 Interceptor 401: un único `POST /auth/refresh` en vuelo (cola de requests), reintenta el original; si refresh falla → limpia sesión y va a `/login`.
- [ ] 2.5 Hidratación optimista al bootear: dispara `refresh`; 200 → entra directo, 401 → `/login`. Sin flash de login (estado `loading`).
- [ ] 2.6 Helper `parseProblemDetails` → mensaje general + `errors` por campo.

## 3. Login y guards

- [ ] 3.1 Página `/login`: form email + password (shadcn `form` + zod), llama `POST /auth/login`, guarda token/user, redirige según `role` (`Admin`/`Profesional` → `/turnos`).
- [ ] 3.2 Manejo de error 401 inline ("credenciales inválidas"); estado de submit.
- [ ] 3.3 `<RequireAuth>`: sin sesión → `/login` (guardando `from`).
- [ ] 3.4 `<RequireRole roles={[...]}>`: rol incorrecto → redirect o 403 UI.
- [ ] 3.5 Logout: `POST /auth/logout`, limpia memoria + `localStorage`, va a `/login`.

## 4. Layout y navegación

- [ ] 4.1 Shell con sidebar/topbar: nombre de usuario + badge de rol + botón logout.
- [ ] 4.2 Menú condicionado por `role`: Profesional ve solo "Mis turnos"; Admin ve Turnos / Pacientes / Profesionales / Nuevo turno. (Gating solo UX.)
- [ ] 4.3 Componentes transversales: `<PageHeader>`, `<DataTable>` (paginación + estado vacío + loading), `<ConfirmDialog>`, `<Toaster>`, `<FieldError>`.
- [ ] 4.4 Hook `useProblemForm` para volcar `errors` de ProblemDetails a los campos del form.

## 5. Turnos — listado (Admin y Profesional)

- [ ] 5.1 Página `/turnos`: `GET /turnos` con paginación; columnas paciente, profesional, inicio (fecha+hora), estado (badge), notas.
- [ ] 5.2 Barra de filtros: rango de fechas, estado, y (solo Admin) profesional y paciente. Sincronizar a query string.
- [ ] 5.3 Título contextual: Admin "Turnos", Profesional "Mis turnos" (el server ya filtra; no mandar `profesionalId`).
- [ ] 5.4 Fila → abre detalle (drawer/dialog) con datos del paciente (nombre, teléfono, obra social) y del turno.

## 6. Turnos — acciones

- [ ] 6.1 "Nuevo turno" (solo Admin): dialog con buscador de paciente (`GET /pacientes?search=`), select de profesional, selector de fecha + selector de hora → `POST /turnos`.
- [ ] 6.2 Manejo de 409 "slot ocupado" inline en el form; 404 paciente/profesional; 400 validación por campo.
- [ ] 6.3 Editar turno (Admin): reusar el form para `PUT /turnos/{id}` (reprogramar / cambiar paciente / notas); revalida 409.
- [ ] 6.4 Cambiar estado: control que muestra solo las transiciones legales según estado actual y rol (mapa de la máquina de estados en el front) → `PATCH /turnos/{id}/estado`.
- [ ] 6.5 Cancelar turno = `PATCH estado = "Cancelado"` con `<ConfirmDialog>`; refrescar listado (libera slot).
- [ ] 6.6 Profesional: en el detalle solo se ofrecen sus transiciones (`Pendiente→Confirmado`, `Confirmado→Atendido`, `→Cancelado`); resto oculto.

## 7. Pacientes (solo Admin)

- [ ] 7.1 Página `/pacientes`: `DataTable` con `GET /pacientes?search=&page=&pageSize=`; búsqueda con debounce.
- [ ] 7.2 Crear / editar en dialog (`nombre`, `apellido`, `telefono`, `obraSocial`) → `POST` / `PUT`; errores por campo desde ProblemDetails.
- [ ] 7.3 Eliminar con `<ConfirmDialog>` → `DELETE`; manejar 409 "tiene turnos activos" con mensaje claro (toast).

## 8. Profesionales (solo Admin)

- [ ] 8.1 Página `/profesionales`: `DataTable` con `GET /profesionales?search=&page=&pageSize=`.
- [ ] 8.2 Crear / editar (`nombre`, `apellido`, `especialidad`) → `POST` / `PUT`.
- [ ] 8.3 Eliminar → `DELETE`; manejar 409 igual que pacientes.

## 9. Pulido y errores

- [ ] 9.1 Estados de carga (skeletons) y vacíos en todas las tablas.
- [ ] 9.2 Toast global para 500 / errores de red; 403 → pantalla "sin permiso".
- [ ] 9.3 Formato de fecha/hora local consistente (helper `formatInicio`).
- [ ] 9.4 Responsive básico (tablas con scroll horizontal en mobile) y foco/accesibilidad en dialogs.
- [ ] 9.5 Revisar que ningún dato sensible quede en `localStorage` (solo `{ nombre, role }`).

## 10. Deploy

- [ ] 10.1 Proyecto en Vercel: framework Vite, `VITE_API_URL` apuntando a Railway.
- [ ] 10.2 Verificar que el dominio de Vercel esté en `Cors__AllowedOrigins` del backend y que la cookie `rt` viaje (SameSite/Secure) entre dominios.
- [ ] 10.3 Smoke en prod: login Admin (crear paciente/profesional/turno, cambiar estado), login Profesional (ver solo lo suyo, cambiar estado), F5 sin flash de login, expiración de token → refresh transparente, logout.

## 11. Documentación

- [ ] 11.1 Sección "Frontend" en el bloque "Cómo correr localmente" del `README.md`.
- [ ] 11.2 Aportar los prompts de IA usados en el front a `PROMPTS.md` / `docs/uso-de-ia.md`.
