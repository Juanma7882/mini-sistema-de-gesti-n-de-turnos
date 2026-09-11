using Turnos.Domain.Profesionales;

namespace Turnos.Application.Abstractions;

/// <summary>Acceso a datos del agregado Profesional. La implementación EF Core
/// (Infrastructure) aplica el filtro global de soft delete: los métodos de
/// lectura nunca devuelven profesionales con <c>DeletedAt != null</c>.</summary>
public interface IProfesionalRepository
{
    /// <summary>Página de profesionales activos. <paramref name="search"/>
    /// (opcional) filtra por <c>Nombre</c> + <c>Apellido</c>, sin distinguir
    /// may/min.</summary>
    Task<(IReadOnlyList<Profesional> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct);

    Task<Profesional?> GetByIdAsync(int id, CancellationToken ct);

    Task AddAsync(Profesional profesional, CancellationToken ct);

    void Update(Profesional profesional);

    /// <summary>Hay al menos un turno del profesional en estado
    /// <c>Pendiente</c> o <c>Confirmado</c>.</summary>
    Task<bool> TieneTurnosActivosAsync(int profesionalId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
