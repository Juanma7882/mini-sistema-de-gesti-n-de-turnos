using Turnos.Domain.Usuarios;

namespace Turnos.Domain.Auth;

/// <summary>
/// Refresh token opaco emitido en el login. El token en claro nunca se persiste:
/// se guarda solo su hash SHA-256 (hex). Rotación <em>revoke-on-use</em>: al usarse
/// se setea <see cref="RevokedAt"/> + <see cref="ReplacedByHash"/> y se emite uno nuevo.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    /// <summary>SHA-256 del token en claro, en hex (64 chars).</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC. <c>null</c> mientras el token sigue vigente.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash del token que lo reemplazó al rotar. <c>null</c> si no se rotó.</summary>
    public string? ReplacedByHash { get; set; }
}
