# Frontend Route Code Splitting

## Purpose

Define qué rutas del SPA se cargan de forma diferida (`React.lazy` + `Suspense`) en `src/app/router.tsx`, con qué fallback visual, y las garantías de comportamiento: los guards de rol/auth (`RequireAuth`, `RequireRole`) se siguen evaluando antes de descargar el chunk, y el shell (`MainLayout`) permanece montado mientras carga.

## Requirements

### Requirement: Rutas exclusivas de Admin se cargan de forma diferida
Las rutas `/pacientes` y `/profesionales`, ambas restringidas a rol Admin por `RequireRole`, SHALL cargarse mediante `import()` dinámico (`React.lazy`) en vez de import estático, de modo que su código no forme parte del chunk inicial descargado por un usuario con rol Profesional.

#### Scenario: Usuario Profesional entra a la app
- **WHEN** un usuario con rol Profesional inicia sesión y carga `/turnos`
- **THEN** el bundle/chunks descargados no incluyen el código de `PacientesPage` ni `ProfesionalesPage`

#### Scenario: Usuario Admin navega a Pacientes
- **WHEN** un usuario Admin, ya autenticado, hace click en el ítem de menú "Pacientes"
- **THEN** el guard `RequireRole` se evalúa antes de disparar la descarga del chunk de `PacientesPage`, y una vez autorizado el chunk se descarga y la página se renderiza

### Requirement: Páginas de error se cargan de forma diferida
`ForbiddenPage` y `NotFoundPage` SHALL cargarse mediante `import()` dinámico, ya que no se visitan en el camino feliz de ningún rol.

#### Scenario: Ruta inexistente
- **WHEN** un usuario autenticado navega a una URL que no matchea ninguna ruta definida
- **THEN** react-router resuelve el catch-all `*`, descarga el chunk de `NotFoundPage` de forma diferida y la renderiza dentro del shell (`MainLayout`)

### Requirement: El shell permanece visible durante la carga del chunk
Mientras una ruta lazy está descargando su chunk, `MainLayout` (navbar/sidebar) SHALL permanecer montado; el fallback de carga SHALL ocupar únicamente el área de contenido (donde iría la ruta), no el viewport completo.

#### Scenario: Transición a una ruta lazy con conexión lenta
- **WHEN** un usuario Admin navega a `/pacientes` y la descarga del chunk tarda de forma perceptible
- **THEN** el navbar y el sidebar de `MainLayout` siguen visibles y solo el área de contenido muestra el indicador de carga

### Requirement: Rutas de alto tráfico quedan fuera del code-splitting
`LoginPage` y `TurnosPage` SHALL seguir importándose de forma estática (no lazy), por ser el punto de entrada y la pantalla inicial de todo usuario autenticado respectivamente.

#### Scenario: Primer render de la app
- **WHEN** la app carga por primera vez (`/login` sin sesión, o `/turnos` con sesión activa)
- **THEN** ni `LoginPage` ni `TurnosPage` muestran un fallback de Suspense — se renderizan de inmediato como parte del bundle inicial
