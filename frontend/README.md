# Frontend — MAE Turnos

React + TypeScript (Vite) · Tailwind CSS v4 · React Router · react-hook-form + zod.
Estructura y convenciones: **`../arquitectura-frontend.md`**. Contrato de la API
(rutas, DTOs, auth): **`../README.md`**. Tareas pendientes: **`../frontend.md`**.

## Correr localmente

```bash
pnpm install
cp .env.example .env      # ajustar VITE_API_URL si hace falta
pnpm dev                  # http://localhost:5173
```

## Scripts

| Script | Qué hace |
|---|---|
| `pnpm dev` | Servidor de desarrollo (Vite). |
| `pnpm build` | `tsc -b` + `vite build` → `dist/`. |
| `pnpm preview` | Sirve el build de `dist/`. |
| `pnpm typecheck` | Chequeo de tipos (`tsc -b`). |
| `pnpm lint` | ESLint (flat config). |
| `pnpm format` | Prettier `--write`. |
| `pnpm test` | Vitest (una corrida). |

## Estado del scaffold

Estructura de carpetas de `arquitectura-frontend.md §2` creada y compilando. El
cliente HTTP (interceptor 401 con cola), el `AuthContext` (hidratación optimista
+ sync entre pestañas), los guards de ruta y los módulos `api/` por feature están
implementados. Las pantallas de listado (`Turnos`, `Pacientes`, `Profesionales`)
renderizan con `DataTable`; los formularios en `*Dialog`, los filtros y el detalle
de turno quedan como stubs con `// TODO(frontend.md §…)`.

Pendiente de tarea aparte (`frontend.md §1.3`): `pnpm dlx shadcn@latest init` y los
componentes en `src/shared/components/ui/` (reescribiendo los imports `@/` a
relativos).
