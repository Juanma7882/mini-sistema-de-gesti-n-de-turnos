## ADDED Requirements

### Requirement: Paleta de color rosa pastel + blanco

El frontend SHALL definir su paleta como tokens CSS en el bloque `@theme` de `frontend/src/styles/index.css`, reemplazando el bloque neutro por defecto de shadcn. El fondo de la aplicación SHALL ser un blanco rosado tenue y las superficies de contenido (tarjetas, tablas, diálogos, drawer) SHALL ser blanco puro. El rosa SHALL usarse como capa de orientación (navegación activa, foco, acción primaria, estados) y NO como relleno de fondo de páginas o tarjetas.

#### Scenario: Tokens de color presentes

- **WHEN** se inspecciona `frontend/src/styles/index.css`
- **THEN** el bloque `@theme` define al menos `--color-canvas` (`#FDF6F8`), `--color-surface` (`#FFFFFF`), `--color-primary` (`#C25C82`), `--color-primary-strong` (`#A63C63`), `--color-primary-tint` (`#F7E4EC`), `--color-foreground` (`#3A2A31`), `--color-muted-foreground` (`#785863`), `--color-border` (`#F0DFE6`), `--color-ring` (`#C25C82`) y `--color-destructive` (`#B23A3A`)
- **AND** el `body` usa `--color-canvas` como fondo y `--color-foreground` como color de texto

#### Scenario: Contraste de la acción primaria

- **WHEN** se renderiza un botón primario con texto blanco sobre `--color-primary`
- **THEN** la relación de contraste entre el texto y el fondo cumple WCAG AA (>= 4.5:1)

#### Scenario: El texto principal no es negro puro

- **WHEN** se renderiza texto de cuerpo
- **THEN** su color es `--color-foreground` (`#3A2A31`, ciruela cálido) y nunca `#000000` ni un negro tintado como `#0B0B0B`

### Requirement: Badges de estado del turno

El sistema SHALL representar cada valor de `EstadoTurno` con un badge de color y un ícono lucide consistentes en toda la aplicación, mediante un componente o mapa único reutilizable.

#### Scenario: Mapa de estados a estilo

- **WHEN** se renderiza el badge de un turno
- **THEN** `Pendiente` usa relleno ámbar claro (`#FBE7D2`) + texto `#8A5A1C` + ícono `clock`
- **AND** `Confirmado` usa relleno rosa (`#F7E1EA`) + texto `#B03B6B` + ícono `check`
- **AND** `Atendido` usa relleno verde salvia (`#DFEEE4`) + texto `#3E7A55` + ícono `clipboard-check`
- **AND** `Cancelado` usa relleno gris malva (`#EEE9EB`) + texto `#6E6169` + ícono `x-circle`

#### Scenario: Definición única

- **WHEN** dos vistas distintas muestran el estado de un turno
- **THEN** ambas obtienen el color y el ícono del mismo módulo compartido (no hay estilos de estado duplicados por vista)

### Requirement: Tipografía Manrope

El frontend SHALL usar la familia **Manrope** como única familia tipográfica, cargada como webfont (Google Fonts o `@fontsource/manrope`) con `system-ui` como fallback, en pesos 400/500/600/700. La escala tipográfica SHALL seguir una razón ~1.2 con tamaños explícitos para título de login, título de página, título de diálogo, cuerpo, label/meta y encabezado de tabla.

#### Scenario: Familia aplicada

- **WHEN** se renderiza cualquier texto de la aplicación
- **THEN** la `font-family` resuelta es `Manrope` con fallback `system-ui`

#### Scenario: Labels en sentence case

- **WHEN** se renderiza un label, encabezado de tabla o texto de meta
- **THEN** el texto está en sentence case y NO en mayúsculas sostenidas (`text-transform: uppercase`)

#### Scenario: Números tabulares en datos

- **WHEN** se renderiza una celda de fecha/hora o un contador (paginación, totales)
- **THEN** la celda aplica `font-variant-numeric: tabular-nums`

### Requirement: Íconos con lucide-react y prohibición de emojis

Toda iconografía de la interfaz SHALL provenir de `lucide-react`. La aplicación SHALL NOT usar caracteres emoji en ningún texto visible, incluidos títulos, botones, estados vacíos, mensajes de error y toasts.

#### Scenario: Estado vacío sin emoji

- **WHEN** una tabla no tiene resultados y se muestra su estado vacío
- **THEN** el estado vacío muestra un ícono lucide (p. ej. `calendar-search`, `users`, `user-search`) y texto, sin ningún emoji

#### Scenario: Toast sin emoji

- **WHEN** se dispara un toast de éxito o de error
- **THEN** el toast muestra un ícono lucide acorde al tipo y ningún emoji

#### Scenario: Búsqueda de emojis en el código de UI

- **WHEN** se hace grep de caracteres emoji sobre `frontend/src`
- **THEN** no aparecen emojis en JSX ni en cadenas de texto visibles

### Requirement: Radios, elevación e inicialización de shadcn/ui

El sistema SHALL definir radios diferenciados por jerarquía (tarjetas/inputs/diálogos `10px`, botones `8px`, badges `6px`, sin formas tipo pill) y una única sombra reservada para overlays (diálogo, drawer, dropdown) con tinte rosa. Las tarjetas sobre el canvas SHALL usar borde de 1px sin sombra. `shadcn/ui` SHALL inicializarse con estos tokens como base y sus componentes generados SHALL vivir en `frontend/src/shared/components/ui/` con imports relativos.

#### Scenario: Sombra solo en overlays

- **WHEN** se renderiza una tarjeta o panel de contenido sobre el canvas
- **THEN** la tarjeta usa borde `1px` con `--color-border` y no aplica `box-shadow`
- **AND** solo los overlays (diálogo, drawer, dropdown) aplican la sombra de overlay con tinte rosa

#### Scenario: Componentes de shadcn con imports relativos

- **WHEN** se agrega un componente con el CLI de shadcn
- **THEN** el archivo queda en `frontend/src/shared/components/ui/` y sus imports usan rutas relativas, no el alias `@/`
