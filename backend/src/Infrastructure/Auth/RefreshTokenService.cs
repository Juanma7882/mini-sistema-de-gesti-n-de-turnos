using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Auth;

namespace Turnos.Infrastructure.Auth;

/// <summary>Refresh token opaco (32 bytes aleatorios en hex). Solo se persiste su
/// hash SHA-256. Rotación <em>revoke-on-use</em>.</summary>
internal sealed class RefreshTokenService(
    IRefreshTokenRepository repository,
    IClock clock,
    IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly int _refreshDays = options.Value.RefreshDays;

    public async Task<IssuedRefreshToken> IssueAsync(int usuarioId, CancellationToken ct)
    {
        var (raw, hash) = GenerarToken();
        var now = clock.UtcNow;
        var expiresAt = now.AddDays(_refreshDays);

        await repository.AddAsync(
            new RefreshToken
            {
                UsuarioId = usuarioId,
                TokenHash = hash,
                CreatedAt = now,
                ExpiresAt = expiresAt,
            },
            ct);
        await repository.SaveChangesAsync(ct);

        return new IssuedRefreshToken(raw, expiresAt);
    }

    public async Task<RefreshRotation> RotateAsync(string rawToken, CancellationToken ct)
    {
        var actual = await repository.GetByHashAsync(Hash(rawToken), ct);
        var now = clock.UtcNow;

        if (actual is null || actual.RevokedAt is not null || actual.ExpiresAt <= now)
        {
            throw new UnauthorizedException("Refresh token inválido o expirado.");
        }

        var (nuevoRaw, nuevoHash) = GenerarToken();
        var expiresAt = now.AddDays(_refreshDays);

        actual.RevokedAt = now;
        actual.ReplacedByHash = nuevoHash;
        repository.Update(actual);

        await repository.AddAsync(
            new RefreshToken
            {
                UsuarioId = actual.UsuarioId,
                TokenHash = nuevoHash,
                CreatedAt = now,
                ExpiresAt = expiresAt,
            },
            ct);
        await repository.SaveChangesAsync(ct);

        return new RefreshRotation(actual.UsuarioId, nuevoRaw, expiresAt);
    }

    public async Task RevokeAsync(string rawToken, CancellationToken ct)
    {
        var actual = await repository.GetByHashAsync(Hash(rawToken), ct);
        if (actual is null || actual.RevokedAt is not null)
        {
            return;
        }

        actual.RevokedAt = clock.UtcNow;
        repository.Update(actual);
        await repository.SaveChangesAsync(ct);
    }

    private static (string Raw, string Hash) GenerarToken()
    {
        var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        return (raw, Hash(raw));
    }

    private static string Hash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
