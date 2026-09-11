using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Domain.Pacientes;

namespace Turnos.Infrastructure.Persistence.Configurations;

internal sealed class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
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
}
