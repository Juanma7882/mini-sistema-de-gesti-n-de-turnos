using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests.Fakes;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public int Emitidos { get; private set; }

    public string CreateAccessToken(Usuario usuario)
    {
        Emitidos++;
        return $"access-{usuario.Id}-{Emitidos}";
    }
}
