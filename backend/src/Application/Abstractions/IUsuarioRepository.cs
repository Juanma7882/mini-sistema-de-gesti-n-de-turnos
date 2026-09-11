using Turnos.Domain.Usuarios;

namespace Turnos.Application.Abstractions;

/// <summary>Acceso a datos del agregado Usuario (login y <c>/auth/me</c>).</summary>
public interface IUsuarioRepository
{
    /// <summary>Usuario por email (case-insensitive); <c>null</c> si no existe.</summary>
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct);

    Task<Usuario?> GetByIdAsync(int id, CancellationToken ct);

    Task AddAsync(Usuario usuario, CancellationToken ct);
}
