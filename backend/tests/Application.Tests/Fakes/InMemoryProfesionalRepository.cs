using Turnos.Application.Abstractions;
using Turnos.Domain.Profesionales;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Repositorio de profesionales en memoria. Imita el filtro global de
/// soft delete.</summary>
public sealed class InMemoryProfesionalRepository : IProfesionalRepository
{
    private readonly List<Profesional> _profesionales;
    private int _nextId;

    public InMemoryProfesionalRepository(params Profesional[] seed)
    {
        _profesionales = seed.ToList();
        _nextId = _profesionales.Count == 0 ? 1 : _profesionales.Max(p => p.Id) + 1;
    }

    public HashSet<int> ConTurnosActivos { get; } = [];

    public int SaveChangesCount { get; private set; }

    public Task<(IReadOnlyList<Profesional> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        IEnumerable<Profesional> query = _profesionales.Where(p => p.Usuario.DeletedAt is null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p =>
                $"{p.Usuario.Nombre} {p.Usuario.Apellido}".Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = query.OrderBy(p => p.Id).ToList();
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Profesional>, int)>((items, ordered.Count));
    }

    public Task<Profesional?> GetByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_profesionales.FirstOrDefault(p => p.Id == id && p.Usuario.DeletedAt is null));

    public Task AddAsync(Profesional profesional, CancellationToken ct)
    {
        profesional.Id = _nextId++;
        _profesionales.Add(profesional);
        return Task.CompletedTask;
    }

    public void Update(Profesional profesional)
    {
    }

    public Task<bool> TieneTurnosActivosAsync(int profesionalId, CancellationToken ct) =>
        Task.FromResult(ConTurnosActivos.Contains(profesionalId));

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
