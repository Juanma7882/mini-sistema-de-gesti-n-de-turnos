## 1. Domain

- [x] 1.1 `Usuario.cs`: agregar `Apellido` (`string`, requerido) y `DeletedAt`
      (`DateTime?`); actualizar el comentario de invariantes.
- [x] 1.2 `Profesional.cs`: quitar `Nombre`, `Apellido`, `DeletedAt`; agregar
      `UsuarioId` (`int`, requerido) y dejar `Usuario` como navegación
      obligatoria (no nullable).
- [x] 1.3 `Usuario.cs`: quitar el scalar `ProfesionalId` (la FK ahora vive en
      `Profesional.UsuarioId`); mantener `Profesional` como navegación inversa
      nullable (`Profesional?`).

## 2. Infrastructure (schema + migración)

- [x] 2.1 `UsuarioConfiguration.cs`: mapear `Apellido` (`HasMaxLength(80)`),
      `DeletedAt`; agregar `HasQueryFilter(u => u.DeletedAt == null)`.
- [x] 2.2 `ProfesionalConfiguration.cs`: quitar mapeo de `Nombre`/`Apellido`;
      invertir la relación con `HasOne(p => p.Usuario).WithOne(u => u.Profesional)
      .HasForeignKey<Profesional>(p => p.UsuarioId)`; cambiar
      `HasQueryFilter` a `p => p.Usuario.DeletedAt == null`.
- [x] 2.3 Generar migración EF (`dotnet ef migrations add
      UnifyUsuarioProfesionalIdentity -p src/Infrastructure -s src/Api`) y
      revisar el SQL generado (columna `UsuarioId` NOT NULL + UNIQUE index,
      columnas viejas eliminadas de `Profesionales`, `Apellido`/`DeletedAt`
      nuevas en `Usuarios`).
- [x] 2.4 Borrar la SQLite local de dev y recrearla (`dotnet ef database
      update` + reseed) — no hay datos reales que preservar.

## 3. Infrastructure (repositorios)

- [x] 3.1 `ProfesionalRepository.GetPagedAsync`: cambiar filtro/orden de
      `p.Nombre`/`p.Apellido` a `p.Usuario.Nombre`/`p.Usuario.Apellido`
      (via `Include(p => p.Usuario)` o proyección con join); mantener
      `AsNoTracking()`.
- [x] 3.2 `ProfesionalRepository.GetByIdAsync`: agregar
      `Include(p => p.Usuario)` para que `BajaAsync` pueda mutar
      `profesional.Usuario.DeletedAt` sobre una entidad trackeada.
- [x] 3.3 `UsuarioRepository.GetByEmailAsync`/`GetByIdAsync`: confirmar que
      siguen funcionando igual (ya incluyen `Profesional`); no requieren
      cambios de `Include` porque `Usuario` es el lado independiente.

## 4. Application

- [x] 4.1 `ProfesionalService.CrearAsync`: setear `usuario.Nombre`/`Apellido`
      directamente desde el request (ya no copiar desde `profesional.Nombre`);
      `profesional.Usuario = usuario` en vez de `usuario.Profesional = profesional`
      (ajustar el fixup de EF Core a la FK invertida — ver design.md §1 y §5).
- [x] 4.2 `ProfesionalService.EditarAsync`: actualizar
      `profesional.Usuario.Nombre`/`Apellido` (requiere que `GetByIdAsync`
      incluya `Usuario`, tarea 3.2); `Especialidad` sigue igual en `Profesional`.
- [x] 4.3 `ProfesionalService.BajaAsync`: setear
      `profesional.Usuario.DeletedAt = clock.UtcNow` en vez de
      `profesional.DeletedAt`; mantener el chequeo de
      `TieneTurnosActivosAsync` y la idempotencia (`if (DeletedAt is not null) return;`
      ahora sobre `profesional.Usuario.DeletedAt`).
- [x] 4.4 `ProfesionalMapper.ToDto`/`ToResumen`: leer `Nombre`/`Apellido` desde
      `profesional.Usuario` en vez de `profesional`.
- [x] 4.5 Revisar `CrearProfesionalRequest.cs` y su validator — sin cambios de
      forma esperados (siguen siendo campos del request), pero confirmar que
      el mapeo a `Usuario`/`Profesional` en el servicio quedó consistente.
- [x] 4.6 `IUsuarioRepository`/`IProfesionalRepository`: agregar los métodos
      que falten si el nuevo flujo de `BajaAsync`/`EditarAsync` los necesita
      (evaluar si `IUsuarioRepository` necesita un `Update` explícito o si
      alcanza con `SaveChangesAsync` del `AppDbContext` compartido, como ya
      documenta el comentario de `CrearAsync`).
- [x] 4.7 `TurnoService.GarantizarProfesionalActivoAsync`: quitar el check
      `profesional.DeletedAt is not null` (columna eliminada); alcanza con
      `profesional is null`, porque el query filter global de `Profesional`
      ya excluye a los dados de baja de `GetByIdAsync`.
- [x] 4.8 `TurnoRepository.GetByIdAsync` y `FiltradoBase`: agregar
      `.ThenInclude(p => p.Usuario)` después de `.Include(t => t.Profesional)`
      — sin esto, `ProfesionalMapper.ToResumen` revienta con
      `NullReferenceException` al mapear `turno.Profesional.Usuario.Nombre`
      (reproducido manualmente en `PATCH /turnos/{id}/estado`).

## 5. Api / Auth

- [x] 5.1 `JwtTokenService.CreateAccessToken`: cambiar
      `usuario.ProfesionalId` por `usuario.Profesional?.Id` para el claim
      `profesionalId` (el scalar se eliminó en la tarea 1.3).
- [x] 5.2 `AuthService.ToMeDto`: cambiar `ProfesionalId = usuario.ProfesionalId`
      por `usuario.Profesional?.Id`; confirmar que `Nombre` sigue devuelto
      correctamente en `/auth/me`.
- [x] 5.3 Revisar `ProfesionalesController`: sin cambios de rutas/contrato
      esperados; solo verificar que compile contra el `ProfesionalDto` nuevo.

## 6. Seed

- [x] 6.1 `DbSeeder.cs`: mover `Nombre`/`Apellido` de los `Profesional` seed a
      sus `Usuario` correspondientes; confirmar que el `Usuario.Nombre` del
      Admin sigue seteado igual que antes.

## 7. Tests

- [x] 7.1 `Application.Tests/Fakes/FakeUsuarioRepository.cs`: adaptar al nuevo
      shape de `Usuario` (`Apellido`, `DeletedAt`) y a la FK invertida.
- [x] 7.2 `Application.Tests/Profesionales/ProfesionalServiceTests.cs`:
      actualizar asserts que leían `Profesional.Nombre`/`Apellido`/`DeletedAt`
      para leer desde `Usuario`; agregar un test que cubra "dar de baja
      deshabilita el login" (comportamiento BREAKING documentado).
- [x] 7.3 `Api.Tests/Support/ApiTestDataExtensions.cs` y `TestData.cs`:
      actualizar builders/fixtures de `Profesional`/`Usuario` al nuevo shape.
- [x] 7.4 `Api.Tests/ValidacionTests.cs`: revisar casos que dependan de campos
      movidos.
- [x] 7.5 `Infrastructure.Tests` (si referencia `Profesional.Nombre`/`DeletedAt`
      o `TurnoRepositoryTests`): actualizar al nuevo schema.
- [x] 7.6 Correr `dotnet build` y `dotnet test` completos; confirmar 0 errores
      y 0 warnings nuevos.

## 8. Verificación manual

- [x] 8.1 Levantar backend + frontend; probar alta, edición y baja de un
      profesional desde `ProfesionalDialog`, confirmando que el nombre
      mostrado y el login del profesional se comportan según lo especificado
      en `specs/professional-account-identity/spec.md`.
- [x] 8.2 Confirmar que un profesional dado de baja no puede loguearse
      (prueba manual de `POST /auth/login` contra ese email).
