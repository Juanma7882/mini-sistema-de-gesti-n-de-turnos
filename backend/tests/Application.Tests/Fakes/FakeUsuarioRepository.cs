using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests.Fakes;

public sealed class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios;

    public FakeUsuarioRepository(params Usuario[] seed) => _usuarios = seed.ToList();

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct) =>
        Task.FromResult(_usuarios.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<Usuario?> GetByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));
}
