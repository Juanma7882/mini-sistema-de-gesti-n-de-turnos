# CLAUDE.md — prueba_tecnica

> CLAUDE.md de proyecto. Complementa (no reemplaza) `~/.claude/CLAUDE.md` global.
> La forma de trabajo global (SDD/OpenSpec + Engram + codebase-memory + skills)
> aplica aquí tal cual.

## Contexto

- **Objetivo:** prueba técnica para MAE Software — **mini sistema de gestión de
  turnos de una clínica**.
- **Deadline:** **sábado 12/09/2026 22:30** (48 h desde la recepción, 10/09).
- **No se espera dedicación full** ni producto comercial: solución funcional,
  organizada, que muestre la forma de trabajar. Priorizar y documentar lo
  pendiente.
- **Entregables (7):** (1) sistema desplegado + credenciales admin y profesional,
  (2) repo con historial + README (correr local, env vars, seed usuarios),
  (3) video ≤ 10 min, (4) lista de tecnologías, (5) doc de decisiones técnicas,
  (6) doc de uso de IA (prompts, partes, revisiones), (7) doc de mejoras futuras.

## Requisitos funcionales

- **Pacientes:** alta con nombre y apellido, teléfono, obra social.
- **Profesionales:** alta con nombre y apellido, especialidad.
- **Turnos:** crear (paciente, profesional, fecha, horario, estado), listar,
  modificar datos y estado.
- **Estados:** Pendiente · Confirmado · Cancelado · Atendido.
- **Regla dura:** un profesional no puede tener dos turnos en la misma fecha y
  horario (los cancelados liberan el slot).
- **Roles:** Administrador (ve/crea/modifica/cancela todos los turnos) ·
  Profesional (solo ve sus propios turnos).
- Todos los datos de demo son ficticios.

## Stack

- **Lenguaje principal:** C# — **.NET 9** (SDK 9.0.307).
- **Backend:** ASP.NET Core Web API, arquitectura por capas ligera
  (Domain / Application / Infrastructure / Api). Deploy en **Railway** (Dockerfile).
- **Frontend:** **React + TypeScript** (Vite). Estilos con **Tailwind CSS** +
  componentes **shadcn/ui** (CLI, componentes copiados al repo). Deploy en **Vercel**.
- **Persistencia:** **SQLite** con EF Core + migraciones.
  ⚠️ En Railway montar un **volume** y apuntar la ruta del `.db` ahí por env var
  (`ConnectionStrings__Default`); el filesystem es efímero sin volume.
- **Auth:** JWT propio (sin librería de Identity). Hash de contraseñas con
  **BCrypt**. Usuarios sembrados (admin + profesional). Token corto, sin refresh.
- **Tests:** xUnit + FluentAssertions. Cubrir la regla anti doble-turno y una
  tira de integración de endpoints (`WebApplicationFactory`).
- **CI:** GitHub Actions (build + test) en el push.

## Variables de entorno

- **Backend:** `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`,
  `ConnectionStrings__Default`, `Cors__AllowedOrigins`, `Seed__AdminPassword`,
  `Seed__ProfessionalPassword`.
- **Frontend:** `VITE_API_URL`.

## Convenciones

- Layout: `src/<Proyecto>/`, `tests/<Proyecto>.Tests/`, `frontend/`.
- Nullable reference types habilitado; warnings as errors en `Directory.Build.props`.
- Commits pequeños y atómicos; mensaje en imperativo (conventional commits).
- `dotnet format` antes de cada commit; ESLint + Prettier en el frontend.
- Cada feature entra por el ciclo OpenSpec: `/opsx:explore` → `/opsx:propose`
  → `/opsx:apply` → `/opsx:archive`.
- **`PROMPTS.md` vivo desde el minuto cero** (requisito 6 de la entrega).

## Estado de la forma de trabajo

- [x] `./CLAUDE.md` de proyecto
- [x] OpenSpec inicializado (`openspec/`, comandos `/opsx:*`)
- [x] `.engram/config.json` con `project_name: prueba_tecnica`
- [x] Git inicializado (rama `main`)
- [ ] codebase-memory indexado (correr "Index this project" cuando exista código)
