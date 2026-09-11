## Context

El scaffold del frontend (`frontend/`, pnpm + Vite 8 + React 19 + TS 6 + Tailwind v4) ya implementa la infraestructura descrita en `arquitectura-frontend.md`: `httpClient` con cola de 401, `AuthContext` con hidratación optimista y `BroadcastChannel`, guards `RequireAuth` / `RequireRole`, hooks `use*Query` por feature, `machine.ts` con test, y `LoginPage` funcional. Faltan las pantallas reales: `TurnoDialog`, `FiltrosTurnos` y `TurnoDetailDrawer` son `return null`; `shadcn/ui` no está inicializado; `styles/index.css` tiene la paleta neutra por defecto.

Restricciones: deadline sáb 12/09/2026 22:30, presupuesto ~48 h para todo el proyecto, ~18–22 h para esta entrega. Convenciones ya fijadas y no negociables en esta entrega: imports relativos (nunca `@/`), sin barrels, un componente por archivo, TS `strict` + `noUncheckedIndexedAccess`, sin librería de data-fetching, access token solo en memoria, gating por rol solo UX. La dirección visual (rosa pastel + blanco, Manrope webfont, íconos lucide, cero emojis) fue elegida por el usuario el 2026-09-10 y está detallada en la memoria Engram de `prueba_tecnica` (obs #170).

## Goals / Non-Goals

**Goals:**

- Un sistema visual propio y coherente (color, tipografía, radios, elevación, iconografía) aplicado de forma consistente en todas las vistas mediante tokens y componentes compartidos.
- Cubrir los flujos del `README.md` para `Admin` y `Profesional`: login, listado y gestión de turnos, CRUD de pacientes y profesionales, páginas de error.
- Mantener el contrato con el backend sin cambios; el frontend solo consume las rutas ya definidas.
- Piso de calidad: responsive a mobile, foco visible, `prefers-reduced-motion`, contraste AA.

**Non-Goals:**

- Rol Paciente / autoservicio (change `add-patient-self-service`, fase 2).
- TanStack Query, cache compartida, dedupe, revalidación.
- Dark mode / theming múltiple.
- Revertir estados terminales del turno.
- Tests E2E; la cobertura de front se limita a `machine.ts`, `useProblemForm`, interceptor 401 y guards.

## Decisions

### D1 — Tokens de color en el `@theme` de Tailwind v4, no `tailwind.config.ts`

Tailwind v4 es CSS-first: los tokens van en un bloque `@theme` dentro de `styles/index.css`. Se reemplazan los 9 tokens neutros actuales por la paleta rosa pastel (ver `frontend-visual-system` spec para los hex exactos). shadcn genera además sus variables (`--background`, `--primary`, `--ring`, …); se mapean a los tokens propios en el mismo archivo para tener una sola fuente de color.

- **Alternativa descartada**: definir la paleta como config JS. Rechazada porque Tailwind v4 desaconseja el config file y el scaffold ya adoptó el enfoque CSS-first.
- **Alternativa descartada**: usar la paleta por defecto de shadcn y "teñirla" con utilidades. Rechazada: produce inconsistencias y no logra el rosa como identidad.

### D2 — El rosa es capa de orientación, no fondo

Riesgo estético identificado: el rosa pastel puede leerse como "wellness/spa" en vez de "clínica". Mitigación de diseño: el rosa se limita a navegación activa, foco, botón primario y badges de estado; el contenido vive sobre blanco puro con densidad alta (body 14px, tablas hairline, `tabular-nums`), texto ciruela `#3A2A31` (no negro), y los estados usan también salvia y ámbar. Así el conjunto lee como herramienta de trabajo clínico.

### D3 — `lucide-react` como única fuente de íconos; cero emojis

`lucide-react` ya es la librería de íconos que asume shadcn/ui. Se prohíbe el emoji en todo texto visible (títulos, botones, `EmptyState`, toasts, mensajes de error). Se agrega un check de grep de emojis en `frontend/src` a la checklist de revisión.

- **Alternativa descartada**: `@radix-ui/react-icons`. Rechazada: set más chico y menos expresivo para estados médicos (`stethoscope`, `heart-pulse`, `clipboard-check`).

### D4 — Manrope como webfont con fallback `system-ui`

Una sola familia para todo (400/500/600/700). Se instala `@fontsource/manrope` (self-hosted, sin request a Google en runtime, mejor para el deploy en Vercel y para privacidad). Fallback `system-ui` para el primer paint.

- **Alternativa descartada**: solo `system-ui` (cero dependencias). Ofrecida al usuario; eligió Manrope por identidad. `system-ui` queda como fallback real, no como opción.
- **Alternativa descartada**: `<link>` a Google Fonts. Rechazada: request de terceros en runtime y FOUT menos controlable que con `@fontsource`.

### D5 — Componente único de estado del turno

Un módulo `shared` (o `features/turnos/components/EstadoBadge.tsx` + un mapa en `machine.ts`/`types.ts`) expone `{ color, fill, icon, label }` por `EstadoTurno`. Tabla, drawer y cualquier otra vista lo consumen; no hay estilos de estado duplicados. El orden de la union se mantiene igual al backend: `'Pendiente' | 'Confirmado' | 'Cancelado' | 'Atendido'`.

### D6 — Acciones como diálogos/drawers, estado en la query string

Ninguna acción (nuevo/editar/detalle) crea segmento de ruta. `TurnoDialog` se abre con `?nuevo=1` o `?editar=<id>`; `TurnoDetailDrawer` con `?turno=<id>`. Los filtros de `/turnos` se sincronizan con `shared/lib/queryString.ts`. Beneficio: enlaces compartibles y back/forward del navegador funcionando sin router anidado extra.

### D7 — shadcn/ui: qué componentes se traen y dónde viven

`init` + estos componentes en `frontend/src/shared/components/ui/`: `button`, `input`, `select`, `dialog`, `sheet` (drawer), `table`, `badge`, `sonner`, `form`, `label`, `dropdown-menu`, `calendar`, `popover`, `command`, `textarea`, `skeleton`, `alert`. Tras generarlos se reescriben los imports `@/` a relativos (regla permanente del proyecto). Son código del repo y se editan libremente para encajar con los tokens.

### D8 — Orden de implementación por camino crítico

1. Sistema visual + `@fontsource/manrope` + `shadcn init` + tokens.
2. Componentes `shared` (`DataTable`, `PageHeader`, `EmptyState`, `FieldError`, `ConfirmDialog`) sobre los nuevos tokens.
3. Shell (`MainLayout`, `Navbar`, `InitialsBadge`) + `providers.tsx` (`ErrorBoundary`, `Toaster`) + `/403` y `*`.
4. `LoginPage` (afinar el existente al sistema visual).
5. Turnos: `TurnosPage` → `FiltrosTurnos` → `DataTable` de turnos → `EstadoBadge` → `TurnoDetailDrawer` → `EstadoControl` → `TurnoDialog` → `ConfirmDialog` de cancelación.
6. Maestros: `PacientesPage` + `PacienteDialog` + eliminación; luego `ProfesionalesPage` reusando lo anterior.
7. Pulido: skeletons y estados vacíos en todas las tablas, responsive, foco/a11y en diálogos, grep anti-emoji.

Admin antes que Profesional porque las vistas de Profesional son un subconjunto (menos filtros, sin botones de alta).

## Risks / Trade-offs

- **[El rosa lee como cosmético/no clínico]** → D2: rosa acotado a orientación + densidad alta + acentos salvia/ámbar + texto ciruela. Revisar con capturas al terminar el shell y la tabla de turnos.
- **[Sin data-fetching library, el refetch tras mutación es manual]** → cada `use<Singular>Mutations` invalida llamando el `refetch` de su `use<Plural>Query`; se acepta el boilerplate para esta entrega. Migración a TanStack Query documentada como mejora futura.
- **[shadcn genera imports con alias `@/`]** → paso explícito de reescritura a relativos en cada componente traído; incluido en la checklist de tasks.
- **[`erasableSyntaxOnly` + ESLint 10 flat config tienen gotchas ya conocidos]** (ver `frontend-architecture` memory) → no usar parameter properties; registrar plugins de React a mano. Mantener `pnpm typecheck && lint && build && test` en verde antes de cada commit.
- **[FOUT al cargar Manrope]** → `@fontsource` self-hosted + `font-display: swap` + fallback `system-ui` con métricas parecidas; el salto es menor.
- **[Selectores CSS que se cancelan]** (advertencia de la skill de diseño) → preferir utilidades Tailwind en el JSX y `cn(...)` para condicionales; evitar reglas `.section`/`.cta` que compitan en especificidad.
- **[Alcance vs. deadline]** → si el tiempo aprieta, el orden D8 garantiza que login + turnos (el core demostrable) estén antes que el pulido de maestros.

## Migration Plan

No hay migración de datos ni de esquema: es UI nueva sobre un contrato existente. Despliegue: build de Vite en Vercel con `VITE_API_URL` apuntando a Railway; verificar que el dominio de Vercel esté en `Cors__AllowedOrigins` y que la cookie `rt` viaje entre dominios (`SameSite=None; Secure`). Rollback: revertir el deploy en Vercel al build anterior; el backend no cambia.

## Open Questions

- ¿El selector de hora del `TurnoDialog` es un `<input type="time">` nativo o un `select` de slots de 15/30 min? (Propuesta: `select` de slots para forzar precisión de minuto consistente; confirmar granularidad con el negocio.)
- ¿La paginación de las tablas usa tamaño de página fijo (p. ej. 20) o seleccionable? (Propuesta: fijo en 20 para esta entrega.)
- ¿`ForbiddenPage` / `NotFoundPage` van dentro del shell (con sidebar) o a pantalla completa? (Propuesta y spec actual: dentro del shell para poder navegar afuera.)
