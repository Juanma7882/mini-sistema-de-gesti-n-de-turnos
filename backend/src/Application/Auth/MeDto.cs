using Turnos.Domain.Usuarios;

namespace Turnos.Application.Auth;

/// <summary>Identidad del usuario autenticado. Es el <c>user</c> de la respuesta
/// de <c>POST /auth/login</c> y el cuerpo de <c>GET /auth/me</c>
/// (<c>{ id, nombre, email, role, profesionalId? }</c>).</summary>
public sealed record MeDto
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public required string Email { get; init; }

    public required Rol Role { get; init; }

    public int? ProfesionalId { get; init; }
}
