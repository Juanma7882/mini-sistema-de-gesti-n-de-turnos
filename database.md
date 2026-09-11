# Base de datos — modelo code-first (EF Core + SQLite)

Alcance: esquema de las 5 entidades del contrato del `README.md` (2 roles:
Administrador y Profesional). El rol **Paciente** no entra acá (ver
`openspec/changes/add-patient-self-service/`).
Stack: SQLite + EF Core (migraciones) · .NET 9 · capas Domain / Infrastructure.

Enfoque **code-first**: las clases de `Domain` + `IEntityTypeConfiguration<T>` en
`Infrastructure` son la fuente de verdad; el esquema se genera con
`dotnet ef migrations`. El `README.md` solo tenía un bosquejo de campos; este
archivo fija el tipo de cada columna y su configuración EF. Las tareas de
implementación viven en `backend.md` §2 (Domain) y §3 (Infrastructure).

---

## Convenciones generales

| Tema | Decisión |
|---|---|
| **PK** | `int Id`, autoincremental (`ValueGeneratedOnAdd`). SQLite: `INTEGER PRIMARY KEY AUTOINCREMENT`. Guid descartado por simplicidad y por `Location` legibles (`/pacientes/5`). |
| **Nombres de tabla** | Los del `DbSet`: `Pacientes`, `Profesionales`, `Usuarios`, `Turnos`, `RefreshTokens`. Sin `[Table]`. |
| **Nombres de columna** | Igual al nombre de propiedad (PascalCase). El filtro del índice parcial usa `"Estado"` tal cual. |
| **Strings** | Siempre `HasMaxLength(...)` explícito + `IsRequired()` cuando no son nullable. SQLite no fuerza el largo, pero queda documentado y portable. |
| **Enums** | Se persisten como `int` con `.HasConversion<int>()`. En el JSON viajan como string (config de `JsonStringEnumConverter` en la Api). |
| **Fechas de auditoría** (`CreatedAt`, `UpdatedAt`, `ExpiresAt`, `RevokedAt`) | `DateTime` en **UTC**, seteadas en la capa Application vía `IClock.UtcNow`. **No** se usa `HasDefaultValueSql("CURRENT_TIMESTAMP")` para mantener el reloj testeable. |
| **`Turno.Inicio`** | `DateTime` **local naïve** (`DateTimeKind.Unspecified`), precisión de minuto (segundos/ms truncados al escribir). Serializa como `"2026-09-15T15:00:00"` (sin `Z` ni offset). |
| **Almacenamiento de `DateTime` en SQLite** | El provider lo guarda como `TEXT` ISO-8601; las comparaciones de rango (`desde`/`hasta`) funcionan lexicográficamente. |
| **Soft delete** | `DeletedAt DateTime?` en `Paciente` y `Profesional` + `HasQueryFilter(e => e.DeletedAt == null)`. El `Turno` no se borra: se pasa a `Cancelado`. |
| **Borrado de FKs** | `DeleteBehavior.Restrict` en todas las relaciones salvo `RefreshToken → Usuario` (`Cascade`). Nunca se hace hard-delete de Paciente/Profesional. |

---

## 1. `Paciente`

| Columna | Tipo C# | Null | Config EF | Tipo SQLite |
|---|---|---|---|---|
| `Id` | `int` | no | PK, identity | `INTEGER PK AUTOINCREMENT` |
| `Nombre` | `string` | no | `IsRequired()`, `HasMaxLength(80)` | `TEXT NOT NULL` |
| `Apellido` | `string` | no | `IsRequired()`, `HasMaxLength(80)` | `TEXT NOT NULL` |
| `Telefono` | `string` | no | `IsRequired()`, `HasMaxLength(30)` | `TEXT NOT NULL` |
| `ObraSocial` | `string` | no | `IsRequired()`, `HasMaxLength(120)` | `TEXT NOT NULL` |
| `CreatedAt` | `DateTime` | no | `IsRequired()` (UTC, vía `IClock`) | `TEXT NOT NULL` |
| `DeletedAt` | `DateTime?` | sí | query filter global | `TEXT NULL` |
| `Turnos` | `ICollection<Turno>` | — | nav inversa de `Turno.Paciente` | — |

---

## 2. `Profesional`

| Columna | Tipo C# | Null | Config EF | Tipo SQLite |
|---|---|---|---|---|
| `Id` | `int` | no | PK, identity | `INTEGER PK AUTOINCREMENT` |
| `Nombre` | `string` | no | `IsRequired()`, `HasMaxLength(80)` | `TEXT NOT NULL` |
| `Apellido` | `string` | no | `IsRequired()`, `HasMaxLength(80)` | `TEXT NOT NULL` |
| `Especialidad` | `string` | no | `IsRequired()`, `HasMaxLength(120)` | `TEXT NOT NULL` |
| `CreatedAt` | `DateTime` | no | `IsRequired()` (UTC) | `TEXT NOT NULL` |
| `DeletedAt` | `DateTime?` | sí | query filter global | `TEXT NULL` |
| `Turnos` | `ICollection<Turno>` | — | nav inversa de `Turno.Profesional` | — |
| `Usuario` | `Usuario?` | — | nav inversa 1:0..1 (FK en `Usuario`) | — |

---

## 3. `Usuario`

| Columna | Tipo C# | Null | Config EF | Tipo SQLite |
|---|---|---|---|---|
| `Id` | `int` | no | PK, identity | `INTEGER PK AUTOINCREMENT` |
| `Nombre` | `string` | no | `IsRequired()`, `HasMaxLength(160)` — display name para `GET /auth/me` (resuelve el `nombre` del Admin, que no tiene `Profesional`) | `TEXT NOT NULL` |
| `Email` | `string` | no | `IsRequired()`, `HasMaxLength(256)`, **índice único** | `TEXT NOT NULL` |
| `PasswordHash` | `string` | no | `IsRequired()`, `HasMaxLength(100)` (BCrypt ≈ 60 chars) | `TEXT NOT NULL` |
| `Rol` | `Rol` (enum) | no | `IsRequired()`, `HasConversion<int>()` | `INTEGER NOT NULL` |
| `ProfesionalId` | `int?` | sí | FK → `Profesional`, `OnDelete(Restrict)` | `INTEGER NULL` |
| `Profesional` | `Profesional?` | — | `HasOne(u => u.Profesional).WithOne(p => p.Usuario).HasForeignKey<Usuario>(u => u.ProfesionalId)` | — |
| `CreatedAt` | `DateTime` | no | `IsRequired()` (UTC) | `TEXT NOT NULL` |
| `RefreshTokens` | `ICollection<RefreshToken>` | — | nav inversa | — |

**Invariante** (validada en seed / capa Application, no en DB): `Rol == Admin ⇒ ProfesionalId == null`; `Rol == Profesional ⇒ ProfesionalId != null`.
*Opcional:* materializarla con `ToTable(t => t.HasCheckConstraint("CK_Usuario_Rol_Profesional", "(\"Rol\" = 0 AND \"ProfesionalId\" IS NULL) OR (\"Rol\" = 1 AND \"ProfesionalId\" IS NOT NULL)"))`.

---

## 4. `Turno`

| Columna | Tipo C# | Null | Config EF | Tipo SQLite |
|---|---|---|---|---|
| `Id` | `int` | no | PK, identity | `INTEGER PK AUTOINCREMENT` |
| `PacienteId` | `int` | no | FK → `Paciente`, `OnDelete(Restrict)` | `INTEGER NOT NULL` |
| `Paciente` | `Paciente` | — | nav | — |
| `ProfesionalId` | `int` | no | FK → `Profesional`, `OnDelete(Restrict)` | `INTEGER NOT NULL` |
| `Profesional` | `Profesional` | — | nav | — |
| `Inicio` | `DateTime` | no | `IsRequired()`; `Kind = Unspecified`; minuto exacto | `TEXT NOT NULL` |
| `Estado` | `EstadoTurno` (enum) | no | `IsRequired()`, `HasConversion<int>()`, `HasDefaultValue(EstadoTurno.Pendiente)` | `INTEGER NOT NULL` |
| `Notas` | `string?` | sí | `HasMaxLength(500)` | `TEXT NULL` |
| `CreatedAt` | `DateTime` | no | `IsRequired()` (UTC) | `TEXT NOT NULL` |
| `UpdatedAt` | `DateTime` | no | `IsRequired()` (UTC); se re-setea en cada update | `TEXT NOT NULL` |

**Índices:**

- `IX_Turnos_ProfesionalId_Inicio` — **único parcial**:
  `HasIndex(t => new { t.ProfesionalId, t.Inicio }).IsUnique().HasFilter("\"Estado\" <> 2")`
  (2 = `Cancelado`). Es la garantía dura del anti doble-turno; la carrera entre dos
  escrituras la corta el índice → `DbUpdateException` → **409**.
- `IX_Turnos_ProfesionalId_PacienteId_Activo` — **único parcial**:
  `HasIndex(t => new { t.ProfesionalId, t.PacienteId }).IsUnique().HasFilter("\"Estado\" < 2")`
  (0 = `Pendiente`, 1 = `Confirmado`). Un paciente no puede tener 2+ turnos **activos**
  con el mismo profesional; los `Cancelado`/`Atendido` (2, 3) no cuentan. Pre-chequeo
  en Application + este índice para la carrera → **409**.
- `IX_Turnos_Inicio` — no único, para el filtro `desde`/`hasta` de `GET /turnos`.
- `IX_Turnos_PacienteId` — no único (lo crea EF por la FK; explicitarlo no cuesta).

---

## 5. `RefreshToken`

| Columna | Tipo C# | Null | Config EF | Tipo SQLite |
|---|---|---|---|---|
| `Id` | `int` | no | PK, identity | `INTEGER PK AUTOINCREMENT` |
| `UsuarioId` | `int` | no | FK → `Usuario`, `OnDelete(Cascade)` | `INTEGER NOT NULL` |
| `Usuario` | `Usuario` | — | nav | — |
| `TokenHash` | `string` | no | `IsRequired()`, `HasMaxLength(64)`, `IsFixedLength()`, **índice único** (SHA-256 hex) | `TEXT NOT NULL` |
| `ExpiresAt` | `DateTime` | no | `IsRequired()` (UTC) | `TEXT NOT NULL` |
| `CreatedAt` | `DateTime` | no | `IsRequired()` (UTC) | `TEXT NOT NULL` |
| `RevokedAt` | `DateTime?` | sí | — | `TEXT NULL` |
| `ReplacedByHash` | `string?` | sí | `HasMaxLength(64)` | `TEXT NULL` |

**Índice:** `IX_RefreshTokens_TokenHash` único.
El token en claro nunca se persiste; se guarda solo el hash. Rotación *revoke-on-use*:
al usarse se setea `RevokedAt` + `ReplacedByHash` y se emite uno nuevo.

---

## Enums (`Domain`)

```csharp
public enum Rol { Admin = 0, Profesional = 1 }

public enum EstadoTurno { Pendiente = 0, Confirmado = 1, Cancelado = 2, Atendido = 3 }
```

Persistidos como `int`. El valor `2` (`Cancelado`) está cableado en el `HasFilter`
del índice parcial — si se reordena el enum, actualizar el filtro.

---

## Esbozo de configuración EF (Fluent API)

Un `IEntityTypeConfiguration<T>` por entidad en `Infrastructure/Persistence/Configurations/`.

```csharp
// TurnoConfiguration.cs  (el caso con más reglas)
public void Configure(EntityTypeBuilder<Turno> b)
{
    b.HasKey(t => t.Id);

    b.Property(t => t.Inicio).IsRequired();
    b.Property(t => t.Estado).IsRequired().HasConversion<int>()
        .HasDefaultValue(EstadoTurno.Pendiente);
    b.Property(t => t.Notas).HasMaxLength(500);
    b.Property(t => t.CreatedAt).IsRequired();
    b.Property(t => t.UpdatedAt).IsRequired();

    b.HasOne(t => t.Paciente).WithMany(p => p.Turnos)
        .HasForeignKey(t => t.PacienteId).OnDelete(DeleteBehavior.Restrict);
    b.HasOne(t => t.Profesional).WithMany(p => p.Turnos)
        .HasForeignKey(t => t.ProfesionalId).OnDelete(DeleteBehavior.Restrict);

    b.HasIndex(t => new { t.ProfesionalId, t.Inicio })
        .IsUnique().HasFilter("\"Estado\" <> 2");
    b.HasIndex(t => new { t.ProfesionalId, t.PacienteId })
        .IsUnique().HasFilter("\"Estado\" < 2");
    b.HasIndex(t => t.Inicio);
}
```

```csharp
// PacienteConfiguration.cs  (Profesional es análogo)
public void Configure(EntityTypeBuilder<Paciente> b)
{
    b.HasKey(p => p.Id);
    b.Property(p => p.Nombre).IsRequired().HasMaxLength(80);
    b.Property(p => p.Apellido).IsRequired().HasMaxLength(80);
    b.Property(p => p.Telefono).IsRequired().HasMaxLength(30);
    b.Property(p => p.ObraSocial).IsRequired().HasMaxLength(120);
    b.Property(p => p.CreatedAt).IsRequired();
    b.HasQueryFilter(p => p.DeletedAt == null);
}
```

```csharp
// UsuarioConfiguration.cs
public void Configure(EntityTypeBuilder<Usuario> b)
{
    b.HasKey(u => u.Id);
    b.Property(u => u.Nombre).IsRequired().HasMaxLength(160);
    b.Property(u => u.Email).IsRequired().HasMaxLength(256);
    b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
    b.Property(u => u.Rol).IsRequired().HasConversion<int>();
    b.HasIndex(u => u.Email).IsUnique();
    b.HasOne(u => u.Profesional).WithOne(p => p.Usuario)
        .HasForeignKey<Usuario>(u => u.ProfesionalId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

```csharp
// RefreshTokenConfiguration.cs
public void Configure(EntityTypeBuilder<RefreshToken> b)
{
    b.HasKey(r => r.Id);
    b.Property(r => r.TokenHash).IsRequired().HasMaxLength(64).IsFixedLength();
    b.Property(r => r.ReplacedByHash).HasMaxLength(64);
    b.Property(r => r.ExpiresAt).IsRequired();
    b.Property(r => r.CreatedAt).IsRequired();
    b.HasIndex(r => r.TokenHash).IsUnique();
    b.HasOne(r => r.Usuario).WithMany(u => u.RefreshTokens)
        .HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
}
```

---

## Migración inicial

```bash
dotnet ef migrations add Init -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
```

**Verificar en el migration generado / en el `.db`:**

1. `CREATE UNIQUE INDEX "IX_Turnos_ProfesionalId_Inicio" ... WHERE "Estado" <> 2`
   y `CREATE UNIQUE INDEX "IX_Turnos_ProfesionalId_PacienteId" ... WHERE "Estado" < 2`
   (los dos índices parciales tienen que salir con su `WHERE`).
2. `CREATE UNIQUE INDEX "IX_Usuarios_Email"` y `"IX_RefreshTokens_TokenHash"`.
3. Las columnas enum (`Estado`, `Rol`) son `INTEGER`.
4. `PRAGMA foreign_keys = ON` está activo (SQLite lo requiere por conexión; EF Core
   lo hace por defecto con el provider).

---

## Resumen de índices

| Tabla | Índice | Único | Filtro |
|---|---|---|---|
| `Usuarios` | `Email` | sí | — |
| `Turnos` | `(ProfesionalId, Inicio)` | sí | `"Estado" <> 2` |
| `Turnos` | `(ProfesionalId, PacienteId)` | sí | `"Estado" < 2` |
| `Turnos` | `Inicio` | no | — |
| `Turnos` | `PacienteId` | no | — (FK) |
| `Turnos` | `ProfesionalId` | no | — (FK, cubierto por el compuesto) |
| `RefreshTokens` | `TokenHash` | sí | — |
| `RefreshTokens` | `UsuarioId` | no | — (FK) |

---

## Decisiones y opcionales

- **Sin token de concurrencia** en `Turno`. La carrera del anti doble-turno ya la
  cubre el índice único parcial. Un `rowversion` real no existe en SQLite; si se
  quisiera, sería un `int Version` como `IsConcurrencyToken()` incrementado a mano.
  Fuera de alcance para la prueba.
- **`Usuario.Nombre`** se agrega respecto del bosquejo del README para poder
  responder `GET /auth/me` con `nombre` también cuando el rol es `Admin`.
- **Check constraint `Rol`/`ProfesionalId`**: opcional; por defecto se valida en
  código (seed + Application) para no acoplar el dominio a SQLite.
- **`CreatedAt`/`UpdatedAt` sin default SQL**: se setean vía `IClock` para tests
  deterministas y para respetar UTC vs. hora local naïve de `Inicio`.
