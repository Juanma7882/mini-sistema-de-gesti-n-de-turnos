namespace Turnos.Application.Auth;

/// <summary>Resultado de <c>LoginAsync</c>: el cuerpo JSON más el refresh token
/// en claro y su expiración, para que la Api arme la cookie <c>rt</c>.</summary>
public sealed record AuthSession
{
    public required AuthResultDto Result { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTime RefreshTokenExpiresAt { get; init; }
}
