## ADDED Requirements

### Requirement: Shell de aplicación con sidebar responsive

Las rutas autenticadas SHALL renderizarse dentro de un `MainLayout` que provee una barra lateral de navegación fija de ~240px en escritorio y colapsa a una barra superior con menú deslizable en viewports menores a 768px. El área de contenido SHALL usar el color de canvas, con un ancho máximo de ~1200px y padding de 24–32px.

#### Scenario: Sidebar en escritorio

- **WHEN** un usuario autenticado abre `/turnos` en un viewport de ancho >= 768px
- **THEN** se muestra la barra lateral con la marca de la clínica, los ítems de navegación y el bloque de usuario
- **AND** el contenido de la ruta se renderiza a la derecha mediante `<Outlet/>`

#### Scenario: Colapso en mobile

- **WHEN** el viewport es menor a 768px
- **THEN** la navegación se presenta como barra superior con un botón `menu` (lucide) que abre un panel deslizable con los mismos ítems

### Requirement: Navbar con identidad de usuario y logout

El `Navbar` SHALL mostrar el nombre del usuario, un `InitialsBadge` con sus iniciales sobre fondo rosa-tint, un badge con su rol y una acción de cerrar sesión con ícono `log-out`.

#### Scenario: Datos del usuario visibles

- **WHEN** hay una sesión activa
- **THEN** el `Navbar` muestra `user.nombre`, sus iniciales en el `InitialsBadge` y un badge cuyo texto es `Admin` o `Profesional`

#### Scenario: Cerrar sesión

- **WHEN** el usuario activa la acción de cerrar sesión
- **THEN** el frontend llama `POST /auth/logout`, limpia el token en memoria y el snapshot de `localStorage`, y navega a `/login`

### Requirement: Menú condicionado por rol (solo UX)

El menú de navegación SHALL mostrar ítems según `user.role`. Un `Profesional` SHALL ver únicamente el ítem de turnos (etiquetado "Mis turnos"). Un `Admin` SHALL ver "Turnos", "Pacientes" y "Profesionales". Este ocultamiento es solo de experiencia de usuario; la API sigue siendo la fuente de verdad de los permisos.

#### Scenario: Menú de Profesional

- **WHEN** el usuario autenticado tiene `role === 'Profesional'`
- **THEN** el menú muestra solo "Mis turnos" (ícono `calendar-days`) y no muestra Pacientes ni Profesionales

#### Scenario: Menú de Admin

- **WHEN** el usuario autenticado tiene `role === 'Admin'`
- **THEN** el menú muestra "Turnos" (`calendar-days`), "Pacientes" (`users`) y "Profesionales" (`stethoscope`)

#### Scenario: Ítem activo resaltado

- **WHEN** la ruta actual coincide con un ítem del menú
- **THEN** ese ítem usa fondo `--color-primary-tint`, una barra lateral de 3px en rosa y texto/ícono en `--color-primary-strong` con peso 600

### Requirement: Estado de carga de sesión sin flash de login

Mientras `AuthContext.status === 'loading'` (hidratación optimista por `POST /auth/refresh` al bootear), la aplicación SHALL mostrar una pantalla de carga a pantalla completa con un spinner (`loader-2` de lucide) en color primario y SHALL NOT renderizar el `MainLayout` ni la pantalla de `/login`.

#### Scenario: Boot con cookie válida

- **WHEN** la app arranca y `status` es `loading`
- **THEN** se muestra el spinner a pantalla completa sin chrome de layout
- **AND** cuando el refresh responde 200 la app navega directo a la ruta pedida sin haber mostrado `/login`

#### Scenario: Boot sin sesión

- **WHEN** la app arranca, el refresh responde 401 y `status` pasa a `anonymous`
- **THEN** la app redirige a `/login`

### Requirement: Feedback global y páginas de error

`providers.tsx` SHALL montar un `ErrorBoundary` raíz y un `Toaster` (sonner) globales. Los toasts de éxito SHALL usar tono verde salvia y los de error tono rojo cálido, cada uno con un ícono lucide y sin emoji. Errores 500 o de red detectados por el interceptor HTTP SHALL producir un único toast genérico sin filtrar el detalle técnico. Las rutas `/403` y `*` SHALL renderizar `ForbiddenPage` y `NotFoundPage` dentro del shell.

#### Scenario: Error de render capturado

- **WHEN** un componente lanza un error durante el render
- **THEN** el `ErrorBoundary` muestra una pantalla con ícono `alert-octagon`, el texto "Algo se rompió al mostrar esta pantalla." y un botón "Recargar"

#### Scenario: Error de servidor o de red

- **WHEN** una petición falla con 500 o error de red
- **THEN** se muestra un toast genérico ("No pudimos conectar con el servidor. Probá de nuevo en un momento.") con ícono `wifi-off` y no se expone el stack ni el cuerpo técnico

#### Scenario: Página 403

- **WHEN** el usuario llega a `/403` (por `RequireRole` o por una respuesta 403)
- **THEN** se muestra `ForbiddenPage` dentro del shell, con ícono `shield-x`, el texto "No tenés permiso para ver esta página." y un enlace primario "Ir a turnos"

#### Scenario: Ruta inexistente

- **WHEN** el usuario navega a una ruta que no existe
- **THEN** se muestra `NotFoundPage` dentro del shell, con ícono `compass`, el texto "No encontramos esta página." y un enlace "Volver al inicio"

### Requirement: Piso de calidad de accesibilidad y movimiento

Todos los elementos interactivos SHALL mostrar un anillo de foco visible de 2px usando `--color-ring` con offset de 2px. Las filas de tabla que abren detalle SHALL ser activables por teclado (Enter). Las animaciones de shimmer y las transiciones de diálogo/drawer SHALL respetar `prefers-reduced-motion` degradando a instantáneas.

#### Scenario: Foco visible

- **WHEN** el usuario navega con teclado a un botón, enlace, input o fila interactiva
- **THEN** el elemento muestra un anillo de foco de 2px en `--color-ring` con offset de 2px

#### Scenario: Movimiento reducido

- **WHEN** el sistema operativo del usuario tiene activado "reducir movimiento"
- **THEN** los skeletons no animan el shimmer y los diálogos/drawers aparecen sin transición de desplazamiento
