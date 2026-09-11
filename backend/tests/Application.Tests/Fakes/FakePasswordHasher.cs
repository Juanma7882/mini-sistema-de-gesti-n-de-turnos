using Turnos.Application.Abstractions;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Hasher de juguete: el "hash" es <c>"hash:" + password</c>.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hash:{password}";

    public bool Verify(string password, string hash) => hash == $"hash:{password}";
}
