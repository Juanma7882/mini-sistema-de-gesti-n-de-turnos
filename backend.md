# Backend — Lista de tareas

Alcance: contrato del `README.md` (2 roles: Administrador y Profesional).
Stack: ASP.NET Core Web API · C# / .NET 9 · capas Domain / Application / Infrastructure / Api ·
SQLite + EF Core · JWT propio + refresh con rotación · BCrypt · xUnit + FluentAssertions.

Orden por camino crítico: **compila y corre** antes que features; features antes que
deploy/CI. Tareas de máx. ~2 h. El rol **Paciente** NO entra acá (ver
`openspec/changes/add-patient-self-service/tasks.md`).

---

## 1. Scaffolding y layout de solución

- [x] 1.1 Reestructurar: `backend/Turnos.sln`, `backend/src/{Domain,Application,Infrastructure,Api}`, `backend/tests/`. Descartada la plantilla `WeatherForecast` que traía `backend/`.
- [x] 1.2 Fijar `TargetFramework` a `net9.0` en todos los `.csproj`; `Nullable` y `ImplicitUsings` enable.
- [x] 1.3 Referencias entre proyectos: Api→Application+Infrastructure, Infrastructure→Application, Application→Domain. Domain sin dependencias.
- [x] 1.4 Borrar endpoint/record `WeatherForecast`; `Program.cs` mínimo que arranca y responde `GET /health` → 200.
- [x] 1.5 Paquetes base: EF Core + `Microsoft.EntityFrameworkCore.Sqlite` + Design, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation`, `Swashbuckle`. Fijar EF/Mvc.Testing a `9.0.x` (sin versión, NuGet trae `10.0.x` → net10, incompatible).
- [ ] 1.6 `.gitignore`: `appsettings.Development.json`, `*.db`, `bin/`, `obj/`. `appsettings.json` con claves vacías/placeholder (sin secretos).

## 2. Domain

- [ ] 2.1 Entidades: `Paciente`, `Profesional` (con `CreatedAt`, `DeletedAt?`).
- [ ] 2.2 Entidad `Usuario` (`Email` único, `PasswordHash`, `Rol`, `ProfesionalId?`) y enum `Rol { Admin, Profesional }`.
- [ ] 2.3 Entidad `Turno` (`PacienteId`, `ProfesionalId`, `Inicio` `DateTime`, `Estado`, `Notas?`, `CreatedAt`, `UpdatedAt`) y enum `EstadoTurno { Pendiente=0, Confirmado=1, Cancelado=2, Atendido=3 }`.
- [ ] 2.4 Entidad `RefreshToken` (`UsuarioId`, `TokenHash` SHA-256, `ExpiresAt`, `RevokedAt?`, `ReplacedByHash?`).
- [ ] 2.5 Máquina de estados: función pura `TransicionesLegales(EstadoTurno)` + `EsTransicionValida(desde, hacia)`. `Cancelado` y `Atendido` terminales.
- [ ] 2.6 Tests unitarios de la máquina de estados (todas las flechas legales e ilegales).

## 3. Infrastructure — persistencia

- [ ] 3.1 `AppDbContext` con `DbSet` de las 5 entidades.
- [ ] 3.2 Configuración EF por entidad: `Estado`/`Rol` como `int`, `Email` índice único, FKs, `DeletedAt` filtro de consulta global (soft delete) en Paciente/Profesional.
- [ ] 3.3 Índice único parcial `UNIQUE (ProfesionalId, Inicio) WHERE Estado <> 2` vía `HasFilter(...)`.
- [ ] 3.4 Migración inicial `Init`; verificar `dotnet ef database update` sobre base limpia crea el `.db` y el índice parcial.
- [ ] 3.5 `ConnectionStrings__Default` desde config; registrar `AppDbContext` en DI.

## 4. Infrastructure — auth y seed

- [ ] 4.1 `IPasswordHasher` + impl BCrypt (`Hash` / `Verify`).
- [ ] 4.2 `IJwtTokenService`: emite access token HS256 con claims `sub`, `email`, `role`, `profesionalId?`; lee `Jwt__*` de config.
- [ ] 4.3 `IRefreshTokenService`: genera token opaco aleatorio, guarda hash SHA-256, valida, **revoke-on-use** (marca `RevokedAt` + `ReplacedByHash` y emite nuevo).
- [ ] 4.4 `IClock` (`UtcNow` / hora local naïve de clínica) para expiraciones y validación "a futuro".
- [ ] 4.5 Seeder: si la base está vacía, crea usuario Admin (`admin@clinica.test`) y Profesional (`dra.gomez@clinica.test`) con password desde `Seed__*`, + pacientes/profesionales/turnos ficticios de demo.

## 5. Application — DTOs, validación, casos de uso

- [ ] 5.1 DTOs de respuesta: `PacienteDto`, `ProfesionalDto`, `TurnoDto` (con `paciente`/`profesional` embebidos), `PagedResult<T>`.
- [ ] 5.2 DTOs de request + validadores FluentValidation: `LoginRequest`, `PacienteRequest`, `ProfesionalRequest`, `TurnoRequest`, `CambiarEstadoRequest`.
- [ ] 5.3 Reglas de validación de `TurnoRequest`: `inicio` obligatorio, a futuro, precisión de minuto; `pacienteId`/`profesionalId` existentes y no dados de baja.
- [ ] 5.4 Casos de uso Pacientes: `ListarPacientes` (search + paginación), `ObtenerPaciente`, `CrearPaciente`, `EditarPaciente`, `BajaPaciente` (soft; 409 si tiene turnos activos).
- [ ] 5.5 Casos de uso Profesionales: análogos a Pacientes (misma regla 409 en baja).
- [ ] 5.6 Casos de uso Turnos: `ListarTurnos` (filtros `desde/hasta/estado/pacienteId/profesionalId` + paginación + filtro forzado por rol Profesional), `ObtenerTurno` (404 si ajeno para Profesional).
- [ ] 5.7 `CrearTurno` / `EditarTurno`: valida existencia y baja de paciente/profesional; anti doble-turno (chequeo + captura `DbUpdateException` del índice → 409); `EditarTurno` excluye el propio `Id`.
- [ ] 5.8 `CambiarEstadoTurno`: valida transición según máquina de estados y rol; `Cancelado` libera el slot; transición ilegal → 409.
- [ ] 5.9 Excepciones de dominio tipadas (`NotFoundException`, `ConflictException`, `ValidationException`, `ForbiddenException`) para mapear a status codes en la Api.

## 6. Api — pipeline y auth

- [ ] 6.1 `Program.cs`: DI de todas las capas, `AddDbContext`, `AddValidatorsFromAssembly`, Swagger con botón Bearer.
- [ ] 6.2 `AddAuthentication().AddJwtBearer(...)` con `Jwt__*`; `AddAuthorization` con policy/roles `Admin` y `Profesional`.
- [ ] 6.3 CORS: origins desde `Cors__AllowedOrigins`, `AllowCredentials` (cookie de refresh).
- [ ] 6.4 Middleware de excepciones → `ProblemDetails` RFC 7807 (400 con `errors` por campo, 401/403/404/409/500).
- [ ] 6.5 Helper para leer `role` y `profesionalId` del `ClaimsPrincipal` (`CurrentUser`).

## 7. Api — endpoints Auth

- [ ] 7.1 `POST /auth/login` → 200 `{ token, user }` + `Set-Cookie: rt` (`httpOnly`, `Secure`, `SameSite`, `Path=/auth/refresh`); 401 credenciales inválidas.
- [ ] 7.2 `POST /auth/refresh` (lee cookie `rt`) → 200 `{ token }` + cookie rotada; 401 si ausente/expirada/revocada.
- [ ] 7.3 `POST /auth/logout` (Bearer) → 204, revoca el `rt` y borra la cookie.
- [ ] 7.4 `GET /auth/me` (Bearer) → 200 `{ id, nombre, email, role, profesionalId? }`.

## 8. Api — endpoints Pacientes y Profesionales (solo Admin)

- [ ] 8.1 `GET /pacientes?search=&page=&pageSize=` y `GET /pacientes/{id}` con `[Authorize(Roles="Admin")]`.
- [ ] 8.2 `POST /pacientes` → 201 + `Location`; `PUT /pacientes/{id}` → 200; `DELETE /pacientes/{id}` → 204 / 409.
- [ ] 8.3 `GET /profesionales` y `GET /profesionales/{id}`: lectura para cualquier autenticado; escritura solo Admin.
- [ ] 8.4 `POST/PUT/DELETE /profesionales` (Admin) → 201/200/204, 409 en baja con turnos activos.

## 9. Api — endpoints Turnos

- [ ] 9.1 `GET /turnos` con filtros + paginación; Admin ve todos, Profesional forzado a los suyos (ignora `?profesionalId`).
- [ ] 9.2 `GET /turnos/{id}` → 404 si no existe o no es del Profesional.
- [ ] 9.3 `POST /turnos` (Admin) → 201 + `Location`; 404 paciente/profesional inexistente; 409 slot ocupado.
- [ ] 9.4 `PUT /turnos/{id}` (Admin) → 200; revalida slot; 409 en colisión.
- [ ] 9.5 `PATCH /turnos/{id}/estado`: Admin cualquier transición legal, Profesional solo las suyas; 409 transición ilegal, 404 turno ajeno.

## 10. Tests

- [ ] 10.1 Setup de tests de integración: `WebApplicationFactory` + SQLite en archivo temporal + seed controlado + helper de login por rol.
- [ ] 10.2 Anti doble-turno: dos `POST` mismo `(profesional, inicio)` → segundo 409; cancelar el primero y re-crear → 201.
- [ ] 10.3 Transiciones de estado: matriz legal/ilegal por rol vía `PATCH` (200 vs 409).
- [ ] 10.4 Autorización: Profesional contra endpoints solo-Admin → 403; Profesional accediendo turno ajeno → 404.
- [ ] 10.5 Auth: login OK / credenciales malas 401; refresh rota la cookie e invalida la anterior; logout revoca.
- [ ] 10.6 Soft delete: baja de paciente/profesional con turno activo → 409; sin turnos → 204 y desaparece del listado.
- [ ] 10.7 Validación: `inicio` en el pasado → 400 con `errors`; body incompleto → 400.

## 11. Deploy y CI

- [ ] 11.1 `Dockerfile` multi-stage (build .NET 9 → runtime); `EXPOSE`, `ASPNETCORE_URLS`.
- [ ] 11.2 En arranque: `db.Database.Migrate()` + seed; `ConnectionStrings__Default` apuntando al volume de Railway (`Data Source=./data/turnos.db`).
- [ ] 11.3 Configurar servicio en Railway: variables (`Jwt__*`, `Cors__AllowedOrigins`, `Seed__*`), volume montado en `./data`.
- [ ] 11.4 GitHub Actions: workflow `build + test` en push/PR (`dotnet restore/build/test`).
- [ ] 11.5 Smoke post-deploy: `login` → `me` → crear paciente → crear turno → cambiar estado contra la URL de Railway.

## 12. Documentación

- [ ] 12.1 Completar en `README.md` las secciones ⚠️: "Cómo correr localmente", "Validaciones principales", "Estructura del proyecto".
- [ ] 12.2 `docs/decisiones-tecnicas.md` y `docs/mejoras-futuras.md`.
- [ ] 12.3 `PROMPTS.md` / `docs/uso-de-ia.md`: prompts principales, qué se revisó y corrigió.
