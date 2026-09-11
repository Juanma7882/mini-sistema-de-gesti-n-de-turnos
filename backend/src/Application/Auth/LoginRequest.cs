namespace Turnos.Application.Auth;

/// <summary>Body de <c>POST /auth/login</c>. Mismo endpoint para Admin y
/// Profesional; la diferencia sale del <c>Rol</c> del usuario.</summary>
public sealed record LoginRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
