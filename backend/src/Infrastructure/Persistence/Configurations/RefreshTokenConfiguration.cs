using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Domain.Auth;

namespace Turnos.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(r => r.Id);

        b.Property(r => r.TokenHash).IsRequired().HasMaxLength(64).IsFixedLength();
        b.Property(r => r.ReplacedByHash).HasMaxLength(64);
        b.Property(r => r.ExpiresAt).IsRequired();
        b.Property(r => r.CreatedAt).IsRequired();

        b.HasIndex(r => r.TokenHash).IsUnique();

        b.HasOne(r => r.Usuario)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
