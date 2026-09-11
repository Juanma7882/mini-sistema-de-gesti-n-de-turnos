# Arquitectura del frontend — capas, layout y nomenclatura

Alcance: cómo se organiza el código del frontend y cómo se nombran las cosas.
Complementa a `frontend.md` (lista de tareas). Cuando esta guía y `frontend.md`
difieran en la estructura de carpetas, **manda esta guía** (`frontend.md §1.5`
quedó desactualizado).
Stack: React + TypeScript · Vite · React Router · Tailwind CSS + shadcn/ui ·
zod · Vitest + Testing Library.

Fuente de verdad del **contrato** (rutas, status codes, DTOs, flujo de auth):
`README.md`. Este documento no lo repite: fija la estructura interna que lo
consume.

---

## 1. Capas y dependencias

Cuatro anillos, las dependencias apuntan **hacia adentro**. Ningún módulo importa
"hacia afuera" ni una feature importa a otra feature.

```
app  ──▶  features/*  ──▶  shared  ──▶  core
 │            │                          ▲
 └────────────┴──────────────────────────┘
```

| Anillo | Responsabilidad | NO hace |
|---|---|---|
| **app** | Bootstrap: `main.tsx`, `App.tsx`, árbol de providers, definición de rutas (`router.tsx`). Compone todo; no tiene lógica de dominio. | Componentes de pantalla, llamadas a la API, reglas de negocio. |
| **core** | Infraestructura transversal viva durante toda la sesión: cliente HTTP con interceptor 401, contexto y guards de auth, sincronización entre pestañas, layout raíz, lectura de `env`. | Conocer entidades del dominio (`Paciente`, `Turno`…). |
| **shared** | Genérico y **sin dominio**, reutilizado por más de una feature: componentes de UI (`DataTable`, `ConfirmDialog`…), hooks (`useDebouncedValue`), helpers (`parseProblemDetails`, `formatInicio`), tipos compartidos (`PagedResult<T>`, `ProblemDetails`). | Importar de `features/` o de `app/`. Tener estado de negocio. |
| **features/&lt;dominio&gt;** | Un dominio de negocio por carpeta (`auth`, `pacientes`, `profesionales`, `turnos`), cada uno autocontenido: `pages/`, `components/`, `api/`, `schemas/`, `hooks/`. | Importar de otra feature. Registrar sus propias rutas (lo hace `app/router.tsx`). |
| **pages** | Páginas sin dominio propio (`NotFoundPage`, `ForbiddenPage`). | Lógica; son casi estáticas. |

**Regla de acceso a datos:** cada feature tiene su carpeta `api/` con **una
función por endpoint** del `README.md` que consume (`listarPacientes`,
`crearTurno`, `cambiarEstadoTurno`…). Esas funciones usan el `httpClient` de
`core/api` y devuelven tipos ya parseados; los componentes nunca llaman `fetch`
directo ni arman URLs a mano. El estado de servidor (listas paginadas, refetch
tras una mutación) se maneja con hooks propios de la feature
(`usePacientesQuery`, `useTurnoMutations`) construidos sobre `httpClient`
— sin librería de data-fetching en esta entrega (ver §5.5).

---

## 2. Layout del proyecto

```
frontend/                          ← junto a backend/
  index.html
  vite.config.ts                   ← plugins: react() + tailwindcss() (Tailwind v4, sin postcss.config)
  components.json                  ← config del CLI de shadcn/ui (se crea al correr `shadcn init`)
  tsconfig.json / tsconfig.app.json / tsconfig.node.json   ← "strict": true, sin path aliases (ver §6)
  .env.example                     ← VITE_API_URL=
  eslint.config.js · .prettierrc
  src/
    main.tsx                       ← createRoot + <App/>
    app/
      App.tsx                      ← <Providers><RouterProvider/></Providers>
      providers.tsx                ← AuthProvider, Toaster, ErrorBoundary raíz
      router.tsx                   ← createBrowserRouter: todas las rutas
    core/
      api/
        httpClient.ts              ← baseURL, credentials:'include', Bearer, interceptor 401 con cola
        problemDetails.ts          ← parseProblemDetails(res) → { title, detail, errors }
      auth/
        AuthContext.tsx            ← { accessToken, user, status } + login/logout/refresh
        useAuth.ts                 ← hook de consumo del contexto
        RequireAuth.tsx            ← guard: sin sesión → /login (guarda `from`)
        RequireRole.tsx            ← guard: <RequireRole roles={['Admin']}>
        authApi.ts                 ← login / refresh / logout / me  (POST /auth/*)
        authBroadcast.ts           ← BroadcastChannel: logout/login sincronizado entre pestañas
        session.ts                 ← token SOLO en memoria + snapshot { nombre, role } en localStorage
      layouts/
        MainLayout.tsx             ← shell con sidebar/topbar + <Outlet/>
        Navbar.tsx                 ← nombre, badge de rol, logout; menú condicionado por rol
        InitialsBadge.tsx
      config/
        env.ts                     ← lee y valida import.meta.env.VITE_API_URL
    shared/
      components/
        DataTable.tsx              ← paginación + loading (skeleton) + estado vacío
        ConfirmDialog.tsx
        PageHeader.tsx
        FieldError.tsx
        EmptyState.tsx
        ui/                        ← componentes generados por shadcn/ui (button, input, dialog, table…)
      hooks/
        useDebouncedValue.ts
        useProblemForm.ts          ← vuelca `errors` de ProblemDetails a los campos del form
        usePagedResource.ts       ← estado genérico: data, loading, page, total, refetch
      lib/
        formatInicio.ts           ← DateTime naïve → fecha/hora local legible
        queryString.ts            ← sync de filtros ↔ search params
        cn.ts                      ← clsx + tailwind-merge
      types/
        api.ts                    ← PagedResult<T>, ProblemDetails, EstadoTurno
    features/
      auth/
        pages/LoginPage.tsx
        schemas/loginSchema.ts
      pacientes/
        pages/PacientesPage.tsx
        components/PacienteDialog.tsx
        api/pacientesApi.ts
        schemas/pacienteSchema.ts
        hooks/usePacientesQuery.ts
        types.ts                   ← PacienteDto, PacienteResumen
      profesionales/               ← análogo a pacientes
      turnos/
        pages/TurnosPage.tsx
        components/
          TurnoDialog.tsx          ← crear / editar (solo Admin)
          TurnoDetailDrawer.tsx    ← detalle + datos del paciente
          EstadoControl.tsx        ← muestra solo transiciones legales (rol + estado)
          FiltrosTurnos.tsx
        api/turnosApi.ts
        schemas/turnoSchema.ts
        hooks/useTurnosQuery.ts
        machine.ts                 ← mapa de transiciones legales de la máquina de estados (espejo del backend)
        types.ts                   ← TurnoDto
    pages/
      NotFoundPage.tsx
      ForbiddenPage.tsx
    styles/
      index.css                    ← @tailwind base/components/utilities + tema
    tests/
      setup.ts                     ← Testing Library + matchers de Vitest
```

Si una feature queda muy chica (p. ej. `auth` es casi solo `LoginPage`), es
aceptable aplanar y omitir las subcarpetas vacías, pero mantener **una sola**
forma dentro de cada feature.

---

## 3. Nomenclatura

### 3.1 Idioma

- **Dominio y contrato en español**, igual que el backend: `Paciente`,
  `ObraSocial`, `EstadoTurno`, `inicio`, `/turnos?desde=`. Los tipos del front
  (`PacienteDto`, `TurnoDto`) replican los nombres del `README.md`.
- **Términos técnicos y de React en inglés**: `Page`, `Dialog`, `Drawer`,
  `Provider`, `Context`, `Guard`, `hook`, `schema`, `Query`, `Mutation`.
- Sin `ñ` ni acentos en identificadores (`Telefono`, no `Teléfono`); sí en el
  texto visible de la UI.

### 3.2 Archivos y carpetas

| Elemento | Convención | Ejemplo |
|---|---|---|
| Carpeta de feature | sustantivo plural, minúscula | `features/pacientes/` |
| Componente React | `PascalCase.tsx`, un componente público por archivo, archivo = componente | `PacienteDialog.tsx` |
| Hook | `useCamelCase.ts` | `useDebouncedValue.ts` |
| Módulo de API de feature | `<dominio>Api.ts` con funciones nombradas | `turnosApi.ts` |
| Schema zod | `<entidad>Schema.ts` | `turnoSchema.ts` |
| Helper / util | `camelCase.ts` | `formatInicio.ts` |
| Tipos de una feature | `types.ts` dentro de la feature | `features/turnos/types.ts` |
| Barrel `index.ts` | **no se usan** (romper ciclos e imports ambiguos) | — |

### 3.3 Sufijos de componente (obligatorios)

| Rol | Sufijo | Ejemplo |
|---|---|---|
| Pantalla enrutada | `Page` | `TurnosPage`, `LoginPage`, `NotFoundPage` |
| Modal shadcn `Dialog` | `Dialog` | `PacienteDialog`, `ConfirmDialog` |
| Panel lateral | `Drawer` | `TurnoDetailDrawer` |
| Proveedor de contexto | `Provider` | `AuthProvider` |
| Componente de ruteo que protege | `Require*` | `RequireAuth`, `RequireRole` |
| Layout con `<Outlet/>` | `Layout` | `MainLayout` |
| Control de formulario reutilizable | descriptivo, sin sufijo fijo | `FieldError`, `EstadoControl` |

### 3.4 Hooks

- `use` + capacidad. De datos de servidor: `use<Plural>Query`
  (`usePacientesQuery`) para lectura; `use<Singular>Mutations`
  (`useTurnoMutations`) para crear/editar/cambiar estado.
- Un hook por archivo, mismo nombre que el archivo.
- Los hooks de feature devuelven un objeto plano
  (`{ data, isLoading, error, refetch }`), nunca una tupla posicional.

### 3.5 Tipos TypeScript

- `PascalCase`. DTOs del backend: sufijo `Dto` y campos **camelCase** exactos al
  JSON (`createdAt`, `obraSocial`, `profesionalId`).
- `EstadoTurno` como union de string literales, en el mismo orden que el backend:
  `'Pendiente' | 'Confirmado' | 'Cancelado' | 'Atendido'`.
- Genéricos compartidos en `shared/types/api.ts`:
  `PagedResult<T> { items: T[]; total: number; page: number; pageSize: number }`,
  `ProblemDetails { type; title; status; detail?; errors?: Record<string,string[]> }`.
- Modelos de formulario: se **derivan del schema zod** con `z.infer<typeof
  turnoSchema>`, no se declaran a mano.

### 3.6 Rutas

- Path en minúsculas, plural, sin trailing slash: `/login`, `/turnos`,
  `/pacientes`, `/profesionales`, `/403`, `*`.
- Las acciones (nuevo turno, editar, detalle) son **dialogs/drawers**, no rutas
  propias; el estado abierto/cerrado puede reflejarse en la query string
  (`?nuevo=1`, `?turno=<id>`) pero no crea segmentos de ruta.
- La definición vive **entera** en `app/router.tsx`; las features no se
  autorregistran.

```
/login                         público                → LoginPage
/                              RequireAuth + MainLayout
  index → redirect /turnos
  /turnos                      Admin | Profesional     → TurnosPage
  /pacientes                   RequireRole Admin       → PacientesPage
  /profesionales               RequireRole Admin       → ProfesionalesPage
/403                           → ForbiddenPage
*                              → NotFoundPage
```

### 3.7 Tailwind y shadcn/ui

- **Tailwind v4**: se carga con el plugin `@tailwindcss/vite` en `vite.config.ts`
  (sin `postcss.config.js`). La config es CSS-first: `styles/index.css` hace
  `@import 'tailwindcss'` y define los tokens en un bloque `@theme`. No hay
  `tailwind.config.ts` salvo que el CLI de shadcn lo pida.
- Clases de utilidad en el JSX; combinaciones condicionales con `cn(...)` de
  `shared/lib/cn.ts`. Sin CSS Modules ni styled-components.
- Los componentes que genera el CLI de shadcn se copian a
  `shared/components/ui/` y **se editan libremente** (son código del repo, no una
  dependencia). Tras generarlos, ajustar los imports que el CLI deja con alias
  `@/` a rutas relativas (ver §6).
- Tokens de color / radios en el `@theme` de `styles/index.css`; un solo tema,
  sin dark mode en esta entrega.

### 3.8 Tests

- Archivos `*.test.tsx` / `*.test.ts` **junto al módulo** que prueban
  (`EstadoControl.tsx` + `EstadoControl.test.tsx`).
- Describe = nombre del sujeto; test = frase en español que describe el
  comportamiento (`it('oculta las transiciones que el rol no puede disparar')`).
- Prioridad de cobertura: `machine.ts` (transiciones legales), `useProblemForm`,
  interceptor 401 del `httpClient`, guards de ruta.

---

## 4. Manejo de errores

La API responde `ProblemDetails` (RFC 7807). El front lo traduce en un solo lugar
y lo reparte.

| Situación | Origen | Tratamiento en la UI |
|---|---|---|
| **400** validación | `errors` por campo | `useProblemForm` vuelca cada `errors[campo]` al `FieldError` correspondiente del form |
| **401** en request normal | interceptor del `httpClient` | dispara **un** `POST /auth/refresh` en vuelo (los demás requests esperan en cola), reintenta el original; si el refresh falla → limpia sesión + redirect `/login` |
| **401** en login | `LoginPage` | error inline "credenciales inválidas" |
| **403** rol incorrecto | respuesta o `RequireRole` | pantalla `/403` (`ForbiddenPage`); el menú ya oculta lo que no corresponde (solo UX) |
| **404** recurso / turno ajeno | página de detalle | estado "no encontrado" + volver al listado |
| **409** slot ocupado / transición ilegal / baja con turnos activos | form o acción | mensaje inline en el form (crear/editar turno) o `toast` (borrar maestro) usando `detail` del ProblemDetails |
| **500** / error de red | interceptor | `toast` global genérico; no se filtra el detalle técnico |

- **`parseProblemDetails(res)`** (en `core/api/problemDetails.ts`) es el único
  parser; devuelve `{ title, detail, errors }` normalizado aunque el body no sea
  un ProblemDetails válido.
- **`ErrorBoundary`** raíz en `app/providers.tsx` para errores de render; muestra
  una pantalla de fallo con botón de recarga.
- El `httpClient` **nunca** hace `throw` de un `Response` crudo: siempre rechaza
  con un `ApiError` tipado `{ status, problem }` que las features pueden
  discriminar por `status`.

---

## 5. Estado, sesión y datos

### 5.1 Token de acceso — solo en memoria

El access token (JWT ~15 min) vive en el estado de `AuthContext`, nunca en
`localStorage` ni `sessionStorage`. Un F5 lo borra a propósito.

### 5.2 Snapshot no sensible

`session.ts` guarda en `localStorage` únicamente `{ nombre, role }` para pintar
el menú correcto al instante en el primer render. No es frontera de seguridad y
se valida contra `/auth/me` apenas hay token.

### 5.3 Hidratación optimista al bootear

`AuthProvider` arranca en `status: 'loading'` y dispara `POST /auth/refresh` con
la cookie `rt`:

- **200** → guarda el token en memoria, `status: 'authenticated'`, la app entra
  directo (sin flash de `/login`).
- **401** → `status: 'anonymous'`, redirect a `/login`.

Los guards (`RequireAuth`) no deciden nada mientras `status === 'loading'`
(renderizan un spinner de pantalla completa).

### 5.4 Sincronización entre pestañas

`authBroadcast.ts` usa `BroadcastChannel('auth')`: al hacer logout (o al fallar
un refresh) en una pestaña, las demás limpian su estado y van a `/login`. Al
loguearse, las demás rehidratan.

### 5.5 Estado de servidor (listas, mutaciones)

Sin librería de data-fetching en esta entrega. Cada feature expone:

- `use<Plural>Query({ search, page, pageSize, ...filtros })`: mantiene
  `data: PagedResult<T>`, `isLoading`, `error`, `refetch`; construido sobre
  `usePagedResource` de `shared/hooks`. Los filtros se sincronizan con la query
  string vía `shared/lib/queryString.ts`.
- `use<Singular>Mutations()`: funciones `crear` / `editar` / `cambiarEstado` /
  `eliminar` que, al resolver, invalidan la query llamando su `refetch` y
  muestran `toast` de éxito.

Búsquedas con `useDebouncedValue` (300 ms). Migrar a TanStack Query queda como
mejora futura si la app crece (cache compartida, dedupe, revalidación).

### 5.6 Gating por rol = solo UX

El menú y los botones se ocultan según `user.role`, pero **la API es la fuente de
verdad**. El front nunca envía `profesionalId`: el backend lo toma del claim del
JWT (ver `README.md` §Autenticación).

---

## 6. Convenciones de código

- **Imports siempre relativos** (`../../shared/lib/cn`), nunca alias `@/`
  (preferencia permanente del proyecto: "para evitarnos problemas luego").
  `tsconfig.json` no define `paths`. Si el CLI de shadcn genera imports con `@/`,
  se reescriben a relativos al copiar el componente.
- TypeScript en `strict`; sin `any` (usar `unknown` + narrowing). `noUncheckedIndexedAccess` habilitado.
- Componentes de función + hooks; nada de clases salvo el `ErrorBoundary`.
- Un componente/hook público por archivo, archivo = nombre del símbolo.
- Estado local con `useState`/`useReducer`; contexto solo para lo verdaderamente
  transversal (auth). Nada de estado global de negocio.
- Formularios: `react-hook-form` + `zodResolver`; el schema zod es la única
  fuente de las reglas de validación del cliente y del tipo del form.
- Fechas: `Turno.inicio` llega como string local naïve
  (`"2026-09-15T15:00:00"`); se formatea para mostrar con `formatInicio` y se
  reenvía tal cual (sin `Date` con zona) al backend.
- `eslint` + `prettier` + script `typecheck` (`tsc --noEmit`) en verde antes de
  cada commit; los tres corren en CI (`build + test`).
- Sin librerías de estilo alternativas, sin `moment`/`dayjs` salvo que haga
  falta (evaluar `Intl.DateTimeFormat` primero).

---

## 7. Checklist rápido de nomenclatura

- [ ] Carpeta por feature de dominio (`auth`, `pacientes`, `profesionales`, `turnos`); nada importa a otra feature
- [ ] Dependencias hacia adentro: `app → features → shared → core`
- [ ] Pantalla enrutada → `<X>Page`; modal → `<X>Dialog`; panel → `<X>Drawer`
- [ ] Un componente/hook público por archivo, archivo = nombre del símbolo; sin barrels
- [ ] Llamadas HTTP solo en `features/<x>/api/<x>Api.ts` sobre `core/api/httpClient`
- [ ] Tipos DTO = nombres del `README.md`, campos camelCase, `EstadoTurno` como union de literales
- [ ] Validación de forms = schema zod; el tipo del form sale de `z.infer`
- [ ] Errores: `parseProblemDetails` único parser · 400 → campos · 401 → refresh en cola · 403 → `/403`
- [ ] Access token solo en memoria · `localStorage` solo `{ nombre, role }`
- [ ] Imports relativos, nunca `@/` · TS `strict` · sin `any`
- [ ] Rutas en minúscula plural, definidas todas en `app/router.tsx`
- [ ] Tests `*.test.tsx` junto al módulo · `machine.ts` y el interceptor 401 con cobertura
