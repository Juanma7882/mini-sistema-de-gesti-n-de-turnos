# Arquitectura del backend — capas, layout y nomenclatura

Alcance: cómo se organiza el código del backend y cómo se nombran las cosas.
Complementa a `backend.md` (lista de tareas) y `database.md` (modelo de datos).
Stack: ASP.NET Core Web API · C# / .NET 9 · EF Core + SQLite · JWT propio · xUnit.

Fuente de verdad del **contrato** (rutas, status codes, DTOs): `README.md`.
Este documento no lo repite: fija la estructura interna que lo implementa.

---

## 1. Capas y dependencias

Cuatro proyectos, dependencias en una sola dirección. `Domain` no referencia nada.

```
Api ──▶ Application ──▶ Domain
 │            ▲
 └──▶ Infrastructure ──┘
```

| Capa | Responsabilidad | NO hace |
|---|---|---|
| **Domain** | Entidades, enums, reglas de negocio puras (máquina de estados), excepción base `DomainException`. | EF, DTOs, HTTP, DI, `async`. |
| **Application** | Casos de uso (servicios por agregado), DTOs, `*Request` + validadores, interfaces de infraestructura (`I<Agregado>Repository`, `IClock`, `IJwtTokenService`…), mappers, catálogo de excepciones. | Tipos de EF Core, `HttpContext`, `IConfiguration`. |
| **Infrastructure** | Implementa las interfaces de Application: repositorios sobre `AppDbContext` + `IEntityTypeConfiguration<T>` + migraciones + seed, BCrypt, JWT, refresh tokens, `SystemClock`. | Definir contratos propios que la Api consuma directo. |
| **Api** | Controllers, pipeline (auth, CORS, Swagger), `IExceptionHandler` → `ProblemDetails`, lectura de claims (`CurrentUser`), composición de DI, `Program.cs`. | Lógica de negocio, acceso directo a `DbContext` (va por Application). |

**Regla de acceso a datos:** un **repositorio por agregado**. La interfaz
(`IPacienteRepository`, `ITurnoRepository`, …) vive en `Application/Abstractions`;
la implementación EF Core (`PacienteRepository`, …) en
`Infrastructure/Persistence/Repositories`. El repo encapsula `Include`,
`AsNoTracking`, los filtros y el `SaveChangesAsync`; el `Service` no ve tipos de
EF Core. El `AppDbContext` queda **interno a Infrastructure** (no se expone a
Application). Sin Unit of Work: cada caso de uso toca un agregado y el repo
persiste. Cada repo queda chico (3-6 métodos).

---

## 2. Layout de la solución

```
backend/                           ← todo el backend vive acá (junto a frontend/)
  Turnos.sln
  src/
    Domain/          Turnos.Domain.csproj
    Application/      Turnos.Application.csproj
    Infrastructure/   Turnos.Infrastructure.csproj
    Api/              Turnos.Api.csproj          ← proyecto de arranque (host EF)
  tests/
    Domain.Tests/     Turnos.Domain.Tests.csproj
    Api.Tests/        Turnos.Api.Tests.csproj    ← integración (WebApplicationFactory)
```

- **Carpeta** = nombre corto (`src/Api`). **Assembly + namespace raíz** = con prefijo
  (`Turnos.Api`). La plantilla `WeatherForecast` (`net8.0`) que traía `backend/` se
  descartó al hacer el scaffolding.
- Comandos de `dotnet` y `dotnet ef` se corren desde `backend/`:
  `dotnet ef ... -p src/Infrastructure -s src/Api`.

### Estructura interna por capa (feature folders por agregado)

```
Domain/
  Pacientes/Paciente.cs
  Profesionales/Profesional.cs
  Usuarios/Usuario.cs · Rol.cs
  Turnos/Turno.cs · EstadoTurno.cs · MaquinaEstados.cs
  Auth/RefreshToken.cs
  Common/DomainException.cs

Application/
  Abstractions/    IPacienteRepository.cs · IProfesionalRepository.cs · ITurnoRepository.cs
                   IUsuarioRepository.cs · IRefreshTokenRepository.cs
                   IClock.cs · IPasswordHasher.cs · IJwtTokenService.cs
                   IRefreshTokenService.cs · ICurrentUser.cs
  Common/          PagedResult.cs · PageRequest.cs · Exceptions/*.cs
  Pacientes/       PacienteService.cs · PacienteDto.cs · PacienteRequest.cs
                   PacienteRequestValidator.cs · PacienteMapper.cs
  Profesionales/   (análogo)
  Turnos/          TurnoService.cs · TurnoDto.cs · TurnoRequest.cs
                   CambiarEstadoRequest.cs · TurnoRequestValidator.cs · TurnoMapper.cs
  Auth/            AuthService.cs · LoginRequest.cs · AuthResultDto.cs · MeDto.cs
  DependencyInjection.cs

Infrastructure/
  Persistence/
    AppDbContext.cs                   ← interno a Infrastructure
    Configurations/PacienteConfiguration.cs · … (uno por entidad)
    Repositories/PacienteRepository.cs · ProfesionalRepository.cs · TurnoRepository.cs
                 UsuarioRepository.cs · RefreshTokenRepository.cs
    Migrations/                       ← generadas
    Seed/DbSeeder.cs
  Auth/BcryptPasswordHasher.cs · JwtTokenService.cs · RefreshTokenService.cs
  Time/SystemClock.cs
  DependencyInjection.cs

Api/
  Controllers/AuthController.cs · PacientesController.cs
              ProfesionalesController.cs · TurnosController.cs
  Auth/CurrentUser.cs · ClaimsPrincipalExtensions.cs
  Errors/DomainExceptionHandler.cs   ← IExceptionHandler
  Filters/ValidationActionFilter.cs
  Options/JwtOptions.cs · CorsOptions.cs · SeedOptions.cs
  Program.cs · appsettings.json
```

Si un agregado queda muy chico, `Domain/Entities` + `Domain/Enums` planos es
aceptable, pero mantener **una sola** de las dos formas en toda la capa.

---

## 3. Nomenclatura

### 3.1 Idioma

- **Dominio y contrato en español**: entidades, propiedades, enums, DTOs, rutas,
  parámetros de query (`Paciente`, `ObraSocial`, `EstadoTurno.Confirmado`,
  `/turnos?desde=`). Coherente con `README.md` y `database.md`.
- **Términos técnicos en inglés**: `Service`, `Handler`, `Validator`, `Options`,
  `Repository` (si existiera), `Dto`, `Request`, `Mapper`, `Exception`.
- Sin `ñ` ni acentos en identificadores (`Telefono`, no `Teléfono`).

### 3.2 Proyectos, namespaces, archivos

| Elemento | Convención | Ejemplo |
|---|---|---|
| Assembly / namespace raíz | `Turnos.<Capa>` | `Turnos.Application` |
| Namespace de feature | raíz + carpeta | `Turnos.Application.Turnos` |
| Declaración de namespace | file-scoped | `namespace Turnos.Application.Turnos;` |
| Archivo | un tipo público por archivo, nombre = tipo | `TurnoService.cs` |
| Enums pequeños | pueden acompañar a la entidad o archivo propio | `EstadoTurno.cs` |

### 3.3 Sufijos de clase (obligatorios)

| Tipo | Sufijo | Ejemplo |
|---|---|---|
| Servicio de caso de uso (Application) | `Service` | `TurnoService`, `AuthService` |
| Repositorio (interfaz / impl) | `I<Agregado>Repository` / `<Agregado>Repository` | `ITurnoRepository` / `TurnoRepository` |
| DTO de respuesta | `Dto` | `TurnoDto`, `PacienteDto`, `MeDto` |
| Modelo de entrada (body) | `Request` | `TurnoRequest`, `CambiarEstadoRequest` |
| Validador FluentValidation | `<Request>Validator` | `TurnoRequestValidator` |
| Mapper estático | `Mapper` | `TurnoMapper` |
| Config EF de una entidad | `<Entidad>Configuration` | `TurnoConfiguration` |
| Controller | `<PluralAgregado>Controller` | `TurnosController` |
| Excepción | `Exception` | `NotFoundException`, `ConflictException` |
| Options bindeadas a config | `Options` | `JwtOptions` |
| Handler de `IExceptionHandler` | `Handler` | `DomainExceptionHandler` |
| Filtro MVC | `Filter` | `ValidationActionFilter` |
| Seeder | `Seeder` | `DbSeeder` |

### 3.4 Interfaces e implementaciones

- Interfaz con `I` + nombre por capacidad: `IClock`, `IJwtTokenService`,
  `IPasswordHasher`, `ICurrentUser`, y un `I<Agregado>Repository` por agregado.
- Implementación de servicio de infra = tecnología concreta, **sin**
  `Impl`/`Default`: `SystemClock`, `BcryptPasswordHasher`, `JwtTokenService`.
- Implementación de repositorio = nombre del agregado: `TurnoRepository`.

### 3.5 Métodos y parámetros

- Todo método con I/O termina en `Async` y recibe `CancellationToken ct` como
  último parámetro (`ct`, no `cancellationToken`, en todo el repo).
- Casos de uso: verbo en español que refleja el contrato —
  `ListarAsync`, `ObtenerAsync`, `CrearAsync`, `EditarAsync`, `BajaAsync`,
  `CambiarEstadoAsync`.
- Un método público por caso de uso del `README.md`. El `TurnoService` es el más
  cargado: `ListarAsync` / `ObtenerAsync` / `CrearAsync` / `EditarAsync` /
  `CambiarEstadoAsync`.

### 3.6 DTOs y `Request`

- `public sealed record` con propiedades `init`. JSON en **camelCase**
  (default de `System.Text.Json`); enums como string
  (`JsonStringEnumConverter`).
- `TurnoRequest` lleva `PacienteId`, `ProfesionalId`, `Inicio`, `Notas?`
  (datos, nunca `Estado`). El estado se cambia solo por `CambiarEstadoRequest`.
- Paginación: `PagedResult<T> { Items, Total, Page, PageSize }` y
  `PageRequest { Page = 1, PageSize = 20 }`.

### 3.7 Entidades

- `public class` (EF necesita setters); propiedades PascalCase = nombre de columna
  (ver `database.md`). Navegaciones con el nombre de la entidad relacionada
  (`Paciente`, `Turnos`).
- Enums en `Domain`, valores PascalCase con número explícito:
  `EstadoTurno { Pendiente = 0, Confirmado = 1, Cancelado = 2, Atendido = 3 }`
  (el `2` está cableado en el filtro del índice parcial).

### 3.8 Rutas HTTP

- Prefijo `/api`, sustantivo en plural, minúsculas, kebab si hace falta:
  `/api/pacientes`, `/api/turnos`, `/api/turnos/{id}/estado`, `/api/auth/login`.
- Ruta **literal en minúsculas** en cada controller
  (`[Route("api/turnos")]`), sin `[controller]` para no depender del casing del
  nombre de la clase.
- Query params en camelCase: `search`, `page`, `pageSize`, `desde`, `hasta`,
  `estado`, `pacienteId`, `profesionalId`.

### 3.9 Migraciones EF

- Nombre PascalCase descriptivo: `Init`, luego `AddNotasToTurno`, etc.
- Una migración por cambio de esquema; no se edita a mano una ya aplicada.

### 3.10 Tests

- Clase `<Sujeto>Tests` (`MaquinaEstadosTests`, `TurnosEndpointsTests`).
- Método `Metodo_Escenario_ResultadoEsperado`
  (`CambiarEstado_DePendienteACancelado_DevuelveOk`,
  `CrearTurno_SlotOcupado_Devuelve409`).

---

## 4. Manejo de errores

Los servicios de Application **lanzan** excepciones tipadas; la Api las traduce.

| Excepción (Application, salvo la 1ª) | Status | Cuerpo |
|---|---|---|
| `DomainException` (base, en `Domain/Common`) | — | clase abstracta, no se lanza directa |
| `ValidationException` | **400** | `ProblemDetails` + `errors` por campo |
| `UnauthorizedException` | **401** | `ProblemDetails` |
| `ForbiddenException` | **403** | `ProblemDetails` |
| `NotFoundException` | **404** | `ProblemDetails` (también turno ajeno para Profesional) |
| `ConflictException` | **409** | `ProblemDetails` (slot ocupado, transición ilegal, baja con turnos) |
| cualquier otra | **500** | `ProblemDetails` genérico |

- **.NET 9**: `AddProblemDetails()` + un `IExceptionHandler`
  (`DomainExceptionHandler`) que mapea el tipo → status. Sin middleware manual.
- Validación de body: `ValidationActionFilter` corre el `IValidator<T>` y, si
  falla, lanza `ValidationException` con el diccionario de errores → 400 unificado.
- La colisión de doble-turno se captura como `DbUpdateException` del índice único
  parcial dentro de `TurnoRepository` (donde vive el `SaveChangesAsync`) y se
  re-lanza como `ConflictException`.

---

## 5. Inyección de dependencias y configuración

- Cada capa expone **un** extension method en `DependencyInjection.cs`:
  `services.AddDomain()` (si hace falta), `AddApplication()`,
  `AddInfrastructure(IConfiguration)`, `AddApi(IConfiguration)`.
  `Program.cs` solo los encadena.
- Servicios de Application y de Infrastructure: `AddScoped`.
  `IClock` → `Singleton`. Validadores: `AddValidatorsFromAssembly`.
- **Options pattern** para todo lo configurable:
  `JwtOptions` (`Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessMinutes`,
  `Jwt__RefreshDays`), `CorsOptions` (`Cors__AllowedOrigins`),
  `SeedOptions` (`Seed__AdminPassword`, `Seed__ProfessionalPassword`).
  Bind con `builder.Services.Configure<T>(config.GetSection("..."))`.
- Nombres de sección = prefijo de la env var con `__` (ver `README.md`).
  Nada de secretos en `appsettings.json` versionado.

---

## 6. Convenciones de código

- `Nullable` y `ImplicitUsings` habilitados en todos los `.csproj`;
  `TargetFramework = net9.0`.
- `namespace` file-scoped; `using` ordenados (System primero); un tipo por archivo.
- `sealed` por defecto en clases que no se heredan (servicios, DTOs, validadores,
  handlers). Entidades EF quedan sin `sealed`.
- Consultas de solo lectura: dentro del repositorio, `AsNoTracking()` + `Include`
  explícito de `Paciente`/`Profesional` (una query con joins, sin N+1). El
  `Service` recibe entidades y las pasa por el mapper.
- Sin lazy loading. Sin AutoMapper: mappers estáticos
  (`PacienteMapper.ToDto(paciente)`).
- Fechas de auditoría en UTC vía `IClock.UtcNow`; `Turno.Inicio` es local naïve
  (`DateTimeKind.Unspecified`), ver `database.md`.

---

## 7. Checklist rápido de nomenclatura

- [ ] Proyecto/namespace: `Turnos.<Capa>` · carpeta corta en `src/`
- [ ] Un tipo público por archivo, archivo = nombre del tipo
- [ ] Caso de uso → `<Agregado>Service.VerboAsync(..., CancellationToken ct)`
- [ ] Acceso a datos → `I<Agregado>Repository` (Application) + `<Agregado>Repository` (Infrastructure)
- [ ] Body → `<Caso>Request` + `<Caso>RequestValidator` · Respuesta → `<X>Dto`
- [ ] Config EF → `<Entidad>Configuration` · Controller → `<Plural>Controller`
- [ ] Interfaz `I<Capacidad>` · impl con nombre concreto, sin `Impl`/`Default`
- [ ] Excepción tipada de Application → status en `DomainExceptionHandler`
- [ ] Ruta literal minúscula `/api/<plural>` · query params camelCase
- [ ] Migración PascalCase · Test `Metodo_Escenario_Resultado`
