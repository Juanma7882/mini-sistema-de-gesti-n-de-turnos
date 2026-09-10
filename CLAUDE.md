# CLAUDE.md — prueba_tecnica

> CLAUDE.md de proyecto. Complementa (no reemplaza) `~/.claude/CLAUDE.md` global.
> La forma de trabajo global (SDD/OpenSpec + Engram + codebase-memory + skills)
> aplica aquí tal cual.

## Contexto

- **Objetivo:** resolver una prueba técnica con entrega en **48 h**.
- **Deadline:** definir fecha/hora exacta al recibir la consigna.
- **Consigna oficial:** _pendiente de pegar aquí (resumen + link/adjunto)._
- **Criterios de evaluación conocidos:** _pendiente._

## Stack

- **Lenguaje:** C# — **.NET 9** (SDK 9.0.307).
- **Tipo de app:** _pendiente (API REST / consola / worker / desktop)._
- **Persistencia:** _pendiente (EF Core + SQLite / InMemory / Postgres)._
- **Tests:** xUnit + FluentAssertions (por defecto).
- **Gestión de solución:** un `.sln` en raíz, proyectos bajo `src/` y `tests/`.

## Convenciones

- Layout: `src/<Proyecto>/`, `tests/<Proyecto>.Tests/`.
- Nullable reference types habilitado; warnings as errors en `Directory.Build.props`.
- Commits pequeños y atómicos; mensaje en imperativo.
- `dotnet format` antes de cada commit.
- Cada feature entra por el ciclo OpenSpec: `/opsx:explore` → `/opsx:propose`
  → `/opsx:apply` → `/opsx:archive`.

## Estado de la forma de trabajo

- [x] `./CLAUDE.md` de proyecto
- [x] OpenSpec inicializado (`openspec/`, comandos `/opsx:*`)
- [x] `.engram/config.json` con `project_name: prueba_tecnica`
- [x] Git inicializado (rama `main`)
- [ ] codebase-memory indexado (correr "Index this project" cuando exista código)
