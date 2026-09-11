using Turnos.Domain.Auth;
using Turnos.Domain.Profesionales;

namespace Turnos.Domain.Usuarios;

/// <summary>
/// Cuenta de acceso e identidad de la persona. Un único login para cualquier
/// rol; la diferencia está en <see cref="Rol"/> y en si tiene
/// <see cref="Profesional"/> asociado. Único dueño de <see cref="Nombre"/> y
/// <see cref="Apellido"/>: un <c>Profesional</c> siempre requiere un
/// <c>Usuario</c>, pero un <c>Usuario</c> no siempre es <c>Profesional</c>
/// (ej. Admin).
/// </summary>
/// <remarks>
/// La FK de la relación 1:1 con <see cref="Profesional"/> vive del lado de
/// <c>Profesional</c> (<c>Profesional.UsuarioId</c>, requerida), no acá,
/// porque el lado obligatorio de la relación es el que la lleva.
/// </remarks>
public class Usuario
{
    public int Id { get; set; }

    /// <summary>Nombre para mostrar en <c>GET /auth/me</c> (también cuando el rol es Admin).</summary>
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. El texto plano nunca se persiste.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    /// <summary>Navegación inversa; <c>null</c> salvo que <see cref="Rol"/> sea <see cref="Rol.Profesional"/>.</summary>
    public Profesional? Profesional { get; set; }

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Marca de baja/deshabilitación de la cuenta. <c>null</c> ⇒ activa.
    /// Cubre tanto "cuenta deshabilitada" (cualquier rol) como "profesional
    /// dado de baja" (mismo evento, mismo flag).</summary>
    public DateTime? DeletedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
