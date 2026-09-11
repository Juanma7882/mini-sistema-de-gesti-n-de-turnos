# Decisiones técnicas

Decisiones no obvias tomadas durante la implementación del backend, con el
porqué. No repite lo que ya está en el README (contratos, modelo de datos,
flujos) — es el complemento "por qué se hizo así" de este proyecto.

---

## Arquitectura

**Capas ligeras, no Clean Architecture "de libro".** Domain → Application →
Infrastructure → Api, cuatro `.csproj` con referencias en una sola dirección.
Sin `IAppDbContext` genérico ni Unit of Work explícito: un repositorio por
agregado (`IPacienteRepository`, `ITurnoRepository`, etc.), cada uno con su
propio `SaveChangesAsync`. Justificación: para el tamaño de este proyecto
(4 agregados, sin transacciones cross-agregado más allá de
Profesional+Usuario) un UoW genérico agrega indirección sin resolver un
problema real; los repos ya comparten el mismo `AppDbContext` *scoped*, así
que dos `AddAsync` seguidos de un solo `SaveChangesAsync` persisten juntos en
una transacción implícita (ver "Profesional crea su propio Usuario" abajo).

**`Application` se construyó contra interfaces desde el día uno**
(`Application/Abstractions`), antes de que existiera una sola línea de
`Infrastructure`. Pagó dividendos reales en la capa Api: `TurnoController` no
tiene ni una línea de lógica de scoping por rol, máquina de estados o anti
doble-turno — todo eso vive en `TurnoService` resuelto vía `ICurrentUser`, y el
controlador solo cablea rutas y `[Authorize]`.

## Auth

**JWT + refresh opaco con rotación, no ASP.NET Identity.** Identity trae
mucho más de lo que este dominio necesita (2 roles fijos, sin registro
self-service de Admin/Profesional en el alcance original). `BCrypt` para el
hash de contraseñas, refresh token de 32 bytes aleatorios con solo el hash
SHA-256 persistido (revocable, revoke-on-use).

**`MapInboundClaims = false` es obligatorio, no cosmético.** `JwtTokenService`
emite los claims `sub`/`role` tal cual los define `JwtRegisteredClaimNames`.
`AddJwtBearer` remapea esos nombres a URIs de esquema por default (compat con
`ClaimsIdentity` viejo) — sin desactivarlo, `[Authorize(Roles = "Admin")]` y la
lectura de `sub` en `CurrentUser` fallan en silencio (401/403 inesperados, no
una excepción clara). Alineado también `RoleClaimType`/`NameClaimType` con lo
que el emisor realmente manda.

**Cookie `rt`: `Path=/api/auth`, no `/api/auth/refresh`.** El diseño original
la restringía al endpoint de refresh. Pero `logout` también necesita leerla
para revocar el token — con un `Path` más angosto el browser nunca la manda
ahí. `SameSite=None` + `Secure` (no condicional al entorno) porque frontend
(Vercel) y backend (Railway) son orígenes distintos: la cookie es
inherentemente cross-site, así que hay que correr hasta el entorno de
desarrollo por https si se la quiere probar tal cual se comporta en producción
(perfil `https` de `launchSettings.json`).

**Profesional crea su propio `Usuario`, atómico con `POST /profesionales`.**
Un profesional nunca debería existir sin poder loguearse. `ProfesionalService.
CrearAsync` arma las dos entidades y hace un solo `SaveChangesAsync` — EF Core
resuelve la FK `Usuario.ProfesionalId` a partir de la navegación
`Usuario.Profesional`, sin necesitar el Id del profesional todavía sin
persistir. `PUT /profesionales/{id}` (editar) no toca credenciales a propósito:
separa "cambiar datos" de "cambiar cómo entrás al sistema".

## Api

**Controladores, no Minimal APIs.** Con 4 grupos de endpoints y reglas de
autorización por rol combinables (`[Authorize]` de clase + `[Authorize(Roles=)]`
por acción, que se combinan con AND), los controladores dan filtros de acción
reutilizables (`ValidationFilter`) y una convención de ruta declarativa
(`[Route("api/[controller]")]` + `LowercaseUrls`) sin reinventar nada.

**`ValidationFilter` unifica dos fuentes de error de forma en una sola.**
`AddApi` desactiva el 400 automático de `[ApiController]`
(`SuppressModelStateInvalidFilter`) para que el filtro sea la única fuente de
verdad. Corre FluentValidation *y* revisa `ModelState` — un valor de enum que
no matchea ningún nombre (`{"estado":"NoExiste"}`) es un fallo de *binding*,
no de FluentValidation, y sin el segundo chequeo dejaba `request` en `null` y
el action reventaba con `NullReferenceException` (500 real, encontrado con
curl antes de escribir el fix — ver `docs/uso-de-ia.md`).

**`ExceptionHandler` (`IExceptionHandler`) mapea 5 excepciones de dominio a
`ProblemDetails` RFC 7807**, más un fallback genérico a 500 que solo incluye
el mensaje real de la excepción en `Development` (nunca el stack trace, ni en
dev).

**`page`/`pageSize` se bindean como `[FromQuery] int` sueltos, no como
`PageRequest` directo.** Bindear el record completo (`[FromQuery] PageRequest
page`) falla: el nombre del parámetro `page` colisiona con la propiedad
`PageRequest.Page` y el model binder de ASP.NET Core no resuelve un query
string plano (`?page=1&pageSize=2` volvía siempre con los defaults del
record). Cada listado arma el `PageRequest` a mano a partir de los dos
primitivos.

## Persistencia

**Soft delete solo en Paciente/Profesional, nunca en Turno.** Un turno no se
"borra": se cancela (`PATCH estado = Cancelado`), que es un evento de negocio
real con su propia semántica (libera el slot, queda en el historial). Baja de
paciente/profesional con turnos activos (`Pendiente`/`Confirmado`) → 409;
tienen `DeletedAt` + query filter global.

**Anti doble-turno y "un paciente no puede tener dos turnos activos con el
mismo profesional" se garantizan en dos capas**, no solo en Application: un
pre-chequeo evita el viaje redondo en el caso común, y un índice único parcial
en SQLite (`WHERE Estado <> Cancelado` / `WHERE Estado < Cancelado`) corta la
carrera real entre dos escrituras concurrentes — el repo traduce la
`DbUpdateException` del índice a `ConflictException` (409), nunca un 500.

## Testing

**Estrategia en capas, deliberadamente distinta en cada nivel:**
`Application.Tests` corre contra fakes en memoria (rápido, sin EF ni Sqlite);
`Infrastructure.Tests` verifica los índices únicos parciales contra SQLite
*real* (un fake no puede probar una constraint de la base); `Api.Tests` corre
la Api real completa (`WebApplicationFactory<Program>`) contra un SQLite
temporal — es el único nivel que prueba el *wiring* end-to-end (pipeline,
auth, routing, serialización), no la lógica de negocio en sí (esa ya está
cubierta en Domain/Application).

**`IntegrationTestFactory` setea sus variables de entorno en el constructor,
no vía `ConfigureWebHost(...ConfigureAppConfiguration...)`.** `AddApi`/
`AddInfrastructure` leen `IConfiguration` de forma *eager* al registrar
servicios (`configuration["Jwt:Secret"]`, `GetConnectionString("Default")`
como variables locales capturadas en closures) — el hook estándar de
`WebApplicationFactory` para overridear configuración de test llega *después*
de que `Program.cs` ya leyó esos valores. Las env vars de proceso sí llegan a
tiempo (`WebApplicationBuilder.CreateBuilder(args)` las lee desde la primera
línea de `Program.cs`). Todas las clases de test comparten una sola colección
xUnit (`[Collection("Api")]`) para que esas env vars nunca se pisen entre
factories corriendo en paralelo.

## Deploy

**Dockerfile multi-stage con `--urls http://+:${PORT:-8080}` en el
`ENTRYPOINT`**, no una `ASPNETCORE_URLS` fija. Railway inyecta `PORT`
dinámicamente; el fallback a 8080 deja la misma imagen usable con
`docker run` local sin variables extra.

**`ASPNETCORE_ENVIRONMENT=Development` en Railway, a propósito** (no el
default `Production`). Decisión explícita del alcance de esta entrega: con
`Production` Swagger queda apagado y el evaluador no puede probar los
endpoints desde la URL pública sin armar requests a mano. Documentado como
trade-off consciente, no como descuido — en un despliegue real esto se
revertiría.
