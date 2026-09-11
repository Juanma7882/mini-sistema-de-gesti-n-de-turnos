using Turnos.Domain.Pacientes;

namespace Turnos.Application.Abstractions;

/// <summary>Acceso a datos del agregado Paciente. La implementación EF Core
/// (Infrastructure) aplica el filtro global de soft delete: los métodos de
/// lectura nunca devuelven pacientes con <c>DeletedAt != null</c>.</summary>
public interface IPacienteRepository
{
    /// <summary>Página de pacientes activos. <paramref name="search"/> (opcional)
    /// filtra por <c>Nombre</c> + <c>Apellido</c>, sin distinguir may/min.</summary>
    Task<(IReadOnlyList<Paciente> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct);

    Task<Paciente?> GetByIdAsync(int id, CancellationToken ct);

    Task AddAsync(Paciente paciente, CancellationToken ct);

    void Update(Paciente paciente);

    /// <summary>Hay al menos un turno del paciente en estado
    /// <c>Pendiente</c> o <c>Confirmado</c>.</summary>
    Task<bool> TieneTurnosActivosAsync(int pacienteId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
