using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Domain.Usuarios;

namespace Turnos.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.HasKey(u => u.Id);

        b.Property(u => u.Nombre).IsRequired().HasMaxLength(160);
        b.Property(u => u.Email).IsRequired().HasMaxLength(256);
        b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        b.Property(u => u.Rol).IsRequired().HasConversion<int>();
        b.Property(u => u.CreatedAt).IsRequired();

        b.HasIndex(u => u.Email).IsUnique();

        b.HasOne(u => u.Profesional)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Usuario>(u => u.ProfesionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
