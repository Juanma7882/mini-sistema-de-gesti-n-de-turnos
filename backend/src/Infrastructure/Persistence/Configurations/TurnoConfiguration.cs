using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Domain.Turnos;

namespace Turnos.Infrastructure.Persistence.Configurations;

internal sealed class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> b)
    {
        b.HasKey(t => t.Id);

        b.Property(t => t.Inicio).IsRequired();
        b.Property(t => t.Estado)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(EstadoTurno.Pendiente);
        b.Property(t => t.Notas).HasMaxLength(500);
        b.Property(t => t.CreatedAt).IsRequired();
        b.Property(t => t.UpdatedAt).IsRequired();

        b.HasOne(t => t.Paciente)
            .WithMany(p => p.Turnos)
            .HasForeignKey(t => t.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(t => t.Profesional)
            .WithMany(p => p.Turnos)
            .HasForeignKey(t => t.ProfesionalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Anti doble-turno: un profesional no puede tener dos turnos no cancelados
        // en el mismo instante (2 = Cancelado).
        b.HasIndex(t => new { t.ProfesionalId, t.Inicio })
            .IsUnique()
            .HasFilter("\"Estado\" <> 2");

        // Un paciente no puede tener 2+ turnos ACTIVOS (Pendiente=0 / Confirmado=1)
        // con el mismo profesional. Cancelado/Atendido (2, 3) no cuentan.
        b.HasIndex(t => new { t.ProfesionalId, t.PacienteId })
            .IsUnique()
            .HasFilter("\"Estado\" < 2");

        // Rango desde/hasta de GET /turnos.
        b.HasIndex(t => t.Inicio);
    }
}
