using Turnos.Domain.Auth;

namespace Turnos.Application.Abstractions;

/// <summary>Acceso a datos de los refresh tokens. Lo consume
/// <c>IRefreshTokenService</c> (Infrastructure), no los servicios de
/// Application directamente.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Token por su hash SHA-256 (hex); <c>null</c> si no existe.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct);

    Task AddAsync(RefreshToken token, CancellationToken ct);

    void Update(RefreshToken token);

    Task SaveChangesAsync(CancellationToken ct);
}
