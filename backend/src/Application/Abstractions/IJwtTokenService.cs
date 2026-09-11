using Turnos.Domain.Usuarios;

namespace Turnos.Application.Abstractions;

/// <summary>Emite el access token JWT HS256 con los claims <c>sub</c>,
/// <c>email</c>, <c>role</c> y <c>profesionalId</c> (este último solo para
/// Profesional). La expiración sale de la config (<c>Jwt__AccessMinutes</c>) y es
/// interna a la implementación.</summary>
public interface IJwtTokenService
{
    string CreateAccessToken(Usuario usuario);
}
