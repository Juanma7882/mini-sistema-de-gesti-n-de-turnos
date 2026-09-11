namespace Turnos.Application.Profesionales;

/// <summary>Body de <c>POST /profesionales</c>. Crea el profesional junto con su
/// cuenta de acceso (<c>Rol.Profesional</c>) en una sola operación: un
/// profesional nunca existe sin usuario. El servicio normaliza <see cref="Nombre"/>
/// y <see cref="Apellido"/> (<c>TextoNormalizer.NombrePropio</c>) y
/// <see cref="Especialidad"/> (<c>TextoNormalizer.TextoLibre</c>) antes de
/// persistir.</summary>
public sealed record CrearProfesionalRequest
{
    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string Especialidad { get; init; } = string.Empty;

    /// <summary>Email de login de la cuenta de acceso. Debe ser único.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Contraseña en texto plano; el servicio la hashea antes de persistir.</summary>
    public string Password { get; init; } = string.Empty;
}
