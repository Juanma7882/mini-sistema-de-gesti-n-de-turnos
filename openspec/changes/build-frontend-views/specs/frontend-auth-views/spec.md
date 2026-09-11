## ADDED Requirements

### Requirement: Pantalla de login

La ruta pública `/login` SHALL renderizar `LoginPage`: una tarjeta blanca centrada (~380px) sobre el canvas rosado, con la marca de la clínica, un encabezado breve y un formulario de email + contraseña construido con `react-hook-form` + `zodResolver` sobre `loginSchema`. El campo de contraseña SHALL ofrecer un toggle de visibilidad con íconos `eye` / `eye-off`.

#### Scenario: Render inicial

- **WHEN** un usuario no autenticado abre `/login`
- **THEN** ve la tarjeta con la marca (`heart-pulse` en un tile rosa-tint), el encabezado "Ingresá a tu cuenta", los campos Email (`mail`) y Contraseña (`lock`) y un botón primario "Ingresar" de ancho completo

#### Scenario: Toggle de contraseña

- **WHEN** el usuario activa el toggle del campo contraseña
- **THEN** el `type` del input alterna entre `password` y `text` y el ícono alterna entre `eye` y `eye-off`

### Requirement: Envío del formulario de login

Al enviar credenciales válidas, `LoginPage` SHALL llamar `POST /auth/login`, guardar el token en memoria y el snapshot `{ nombre, role }` en `localStorage`, y redirigir según rol. Durante el envío el botón SHALL mostrar un spinner (`loader-2`), el texto "Ingresando" y quedar deshabilitado.

#### Scenario: Login exitoso de Admin

- **WHEN** el login responde 200 con `user.role === 'Admin'` y no hay `from` guardado
- **THEN** la app navega a `/turnos`

#### Scenario: Login exitoso con destino previo

- **WHEN** el login responde 200 y existe una ubicación `from` guardada por `RequireAuth`
- **THEN** la app navega a `from`

#### Scenario: Estado de envío

- **WHEN** la petición de login está en curso
- **THEN** el botón muestra `loader-2` girando, el texto "Ingresando" y está deshabilitado

### Requirement: Manejo de errores de login

Un 401 en el login SHALL mostrarse como alerta inline sobre el formulario, con ícono `alert-circle`, texto `--color-destructive` sobre fondo rosado claro y el mensaje "Email o contraseña incorrectos." sin lenguaje de disculpa. Un 400 de validación SHALL volcar los errores por campo mediante `FieldError`.

#### Scenario: Credenciales inválidas

- **WHEN** `POST /auth/login` responde 401
- **THEN** se muestra la alerta inline "Email o contraseña incorrectos." con ícono `alert-circle` y el formulario permanece editable

#### Scenario: Validación por campo

- **WHEN** `POST /auth/login` responde 400 con `errors` por campo
- **THEN** cada mensaje aparece bajo su input como `FieldError` con ícono `alert-circle` de 14px

### Requirement: Responsive del login

En viewports menores a 420px la tarjeta de login SHALL ocupar el ancho disponible con márgenes laterales de ~20px.

#### Scenario: Login en pantalla angosta

- **WHEN** el viewport tiene menos de 420px de ancho
- **THEN** la tarjeta de login se muestra a ancho completo con gutter de ~20px y sin scroll horizontal
