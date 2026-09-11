using Microsoft.EntityFrameworkCore;
using Turnos.Application.Abstractions;
using Turnos.Domain.Auth;

namespace Turnos.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public void Update(RefreshToken token) => db.RefreshTokens.Update(token);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
