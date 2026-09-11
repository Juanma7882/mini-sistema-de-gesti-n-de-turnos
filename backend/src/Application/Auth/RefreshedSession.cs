namespace Turnos.Application.Auth;

/// <summary>Resultado de <c>RefreshAsync</c>: el nuevo access token más el
/// refresh token rotado (en claro) y su expiración, para reescribir la cookie.</summary>
public sealed record RefreshedSession
{
    public required string Token { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTime RefreshTokenExpiresAt { get; init; }
}
