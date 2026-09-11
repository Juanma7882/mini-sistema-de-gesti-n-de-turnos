namespace Turnos.Application.Abstractions;

/// <summary>
/// Ciclo de vida del refresh token opaco: emisión, rotación <em>revoke-on-use</em>
/// y revocación. Encapsula la generación aleatoria, el hash SHA-256 y la
/// persistencia (vía <c>IRefreshTokenRepository</c>). Implementación en
/// Infrastructure.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Genera un token nuevo para el usuario, persiste su hash y
    /// devuelve el valor en claro (única vez que se conoce).</summary>
    Task<IssuedRefreshToken> IssueAsync(int usuarioId, CancellationToken ct);

    /// <summary>Valida el token en claro y lo rota: marca el actual como
    /// revocado + <c>ReplacedByHash</c> y emite uno nuevo. Lanza
    /// <c>UnauthorizedException</c> si falta, no existe, expiró o ya fue
    /// revocado.</summary>
    Task<RefreshRotation> RotateAsync(string rawToken, CancellationToken ct);

    /// <summary>Revoca el token en claro si sigue vigente. No-op si no existe o
    /// ya estaba revocado (logout es idempotente).</summary>
    Task RevokeAsync(string rawToken, CancellationToken ct);
}

/// <summary>Token recién emitido: valor en claro para mandar al cliente y su
/// expiración (para la cookie).</summary>
public sealed record IssuedRefreshToken(string RawToken, DateTime ExpiresAt);

/// <summary>Resultado de rotar: a quién pertenece la cadena y el token nuevo.</summary>
public sealed record RefreshRotation(int UsuarioId, string RawToken, DateTime ExpiresAt);
