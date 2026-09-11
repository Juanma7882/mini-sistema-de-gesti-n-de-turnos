using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Domain.Profesionales;

namespace Turnos.Infrastructure.Persistence.Configurations;

internal sealed class ProfesionalConfiguration : IEntityTypeConfiguration<Profesional>
{
    public void Configure(EntityTypeBuilder<Profesional> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Especialidad).IsRequired().HasMaxLength(120);
        b.Property(p => p.CreatedAt).IsRequired();

        b.HasOne(p => p.Usuario)
            .WithOne(u => u.Profesional)
            .HasForeignKey<Profesional>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasQueryFilter(p => p.Usuario.DeletedAt == null);
    }
}
