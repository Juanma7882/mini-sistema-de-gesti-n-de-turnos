using Turnos.Domain.Auth;
using Turnos.Domain.Profesionales;

namespace Turnos.Domain.Usuarios;

/// <summary>
/// Cuenta de acceso. Un único login para ambos roles; la diferencia está en
/// <see cref="Rol"/> y en si tiene <see cref="ProfesionalId"/>.
/// </summary>
/// <remarks>
/// Invariante (validada en seed / Application, no en la DB):
/// <c>Rol == Admin ⇒ ProfesionalId == null</c>;
/// <c>Rol == Profesional ⇒ ProfesionalId != null</c>.
/// </remarks>
public class Usuario
{
    public int Id { get; set; }

    /// <summary>Nombre para mostrar en <c>GET /auth/me</c> (también cuando el rol es Admin).</summary>
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. El texto plano nunca se persiste.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    /// <summary>FK al profesional cuando <see cref="Rol"/> es <see cref="Rol.Profesional"/>; <c>null</c> para Admin.</summary>
    public int? ProfesionalId { get; set; }

    public Profesional? Profesional { get; set; }

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
