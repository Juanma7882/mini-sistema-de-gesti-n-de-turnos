using Microsoft.EntityFrameworkCore;
using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioRepository(AppDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalizado = email.Trim().ToLower();
        return db.Usuarios
            .Include(u => u.Profesional)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizado, ct);
    }

    public Task<Usuario?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Usuarios
            .Include(u => u.Profesional)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct) =>
        await db.Usuarios.AddAsync(usuario, ct);
}
