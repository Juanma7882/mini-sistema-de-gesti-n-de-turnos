## ADDED Requirements

### Requirement: Alta pública de usuario Paciente

El sistema SHALL exponer `POST /auth/register` como endpoint anónimo que crea,
en una única transacción, un `Usuario` con `Rol = Paciente` y su registro
`Paciente` vinculado 1‑a‑1 mediante `Usuario.PacienteId`. Si la transacción
falla en cualquier paso, no SHALL persistir ni el usuario ni el paciente.

El request body SHALL ser
`{ email, password, nombre, apellido, telefono, obraSocial }`.
La respuesta de éxito SHALL ser **201** con
`{ token, user: { id, nombre, email, role: "Paciente", pacienteId } }` y
`Set-Cookie: rt` (mismo mecanismo de refresh token que `POST /auth/login`), de
modo que el paciente queda logueado tras registrarse.

#### Scenario: Registro exitoso

- **WHEN** un visitante anónimo envía `POST /auth/register` con email no usado,
  password válida y datos de paciente completos
- **THEN** el sistema crea `Usuario(Rol=Paciente)` + `Paciente` en una
  transacción, responde **201** con `token`, `user.pacienteId` apuntando al
  paciente creado y setea la cookie `rt`

#### Scenario: Email ya registrado

- **WHEN** el email enviado ya existe en `Usuario`
- **THEN** el sistema responde **409** `ProblemDetails` con
  `title = "Email ya registrado"` y no crea usuario ni paciente

#### Scenario: Password no cumple la política

- **WHEN** la password tiene menos de 8 caracteres o no incluye al menos una
  letra y un dígito
- **THEN** el sistema responde **400** `ProblemDetails` con `errors.password`
  describiendo la regla y no crea nada

#### Scenario: Datos de paciente incompletos

- **WHEN** falta `nombre`, `apellido`, `telefono` u `obraSocial`, o `telefono`
  no cumple el formato aceptado
- **THEN** el sistema responde **400** `ProblemDetails` con `errors` por campo y
  no crea nada

#### Scenario: La password se guarda hasheada

- **WHEN** se crea un usuario paciente vía registro
- **THEN** `Usuario.PasswordHash` contiene un hash BCrypt y nunca la password en
  claro

### Requirement: El registro no permite elegir rol

El endpoint `POST /auth/register` SHALL ignorar cualquier campo de rol presente
en el body y asignar siempre `Rol = Paciente`. No SHALL existir vía pública para
crear usuarios `Admin` o `Profesional`.

#### Scenario: Intento de escalar rol en el body

- **WHEN** el body incluye `"role": "Admin"` o `"rol": "Admin"`
- **THEN** el usuario creado tiene `Rol = Paciente` y la respuesta refleja
  `role: "Paciente"`
