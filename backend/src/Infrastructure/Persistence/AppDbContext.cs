using Microsoft.EntityFrameworkCore;
using Turnos.Domain.Auth;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core. Interno a Infrastructure: la capa Application solo lo ve a
/// través de los repositorios. Las reglas de mapeo viven en los
/// <c>IEntityTypeConfiguration&lt;T&gt;</c> de <c>Persistence/Configurations</c>.
/// </summary>
internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Paciente> Pacientes => Set<Paciente>();

    public DbSet<Profesional> Profesionales => Set<Profesional>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Turno> Turnos => Set<Turno>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
