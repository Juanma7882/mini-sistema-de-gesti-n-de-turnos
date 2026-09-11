namespace Turnos.Application.Auth;

/// <summary>Cuerpo JSON de <c>POST /auth/login</c>: el access token y el usuario.
/// El refresh token no va acá: viaja en una cookie <c>httpOnly</c> que arma la
/// Api con los datos de <see cref="AuthSession"/>.</summary>
public sealed record AuthResultDto
{
    public required string Token { get; init; }

    public required MeDto User { get; init; }
}
