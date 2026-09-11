using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Emite tokens <c>rt-N</c> incrementales y rota con revoke-on-use.</summary>
public sealed class FakeRefreshTokenService(FakeClock clock) : IRefreshTokenService
{
    private readonly Dictionary<string, int> _activos = new();
    private readonly HashSet<string> _revocados = [];
    private int _seq;

    public Task<IssuedRefreshToken> IssueAsync(int usuarioId, CancellationToken ct)
    {
        var raw = $"rt-{++_seq}";
        _activos[raw] = usuarioId;
        return Task.FromResult(new IssuedRefreshToken(raw, clock.UtcNow.AddDays(7)));
    }

    public Task<RefreshRotation> RotateAsync(string rawToken, CancellationToken ct)
    {
        if (!_activos.TryGetValue(rawToken, out var usuarioId))
        {
            throw new UnauthorizedException("Refresh token inválido.");
        }

        _activos.Remove(rawToken);
        _revocados.Add(rawToken);

        var nuevo = $"rt-{++_seq}";
        _activos[nuevo] = usuarioId;
        return Task.FromResult(new RefreshRotation(usuarioId, nuevo, clock.UtcNow.AddDays(7)));
    }

    public Task RevokeAsync(string rawToken, CancellationToken ct)
    {
        _activos.Remove(rawToken);
        _revocados.Add(rawToken);
        return Task.CompletedTask;
    }

    public bool EstaRevocado(string rawToken) => _revocados.Contains(rawToken);
}
