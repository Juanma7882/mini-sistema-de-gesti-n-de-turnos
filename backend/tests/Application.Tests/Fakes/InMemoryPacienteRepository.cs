using Turnos.Application.Abstractions;
using Turnos.Domain.Pacientes;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Repositorio de pacientes en memoria. Imita el filtro global de soft
/// delete: las lecturas no devuelven pacientes con <c>DeletedAt != null</c>.</summary>
public sealed class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _pacientes;
    private int _nextId;

    public InMemoryPacienteRepository(params Paciente[] seed)
    {
        _pacientes = seed.ToList();
        _nextId = _pacientes.Count == 0 ? 1 : _pacientes.Max(p => p.Id) + 1;
    }

    /// <summary>Ids que <see cref="TieneTurnosActivosAsync"/> considera con turnos activos.</summary>
    public HashSet<int> ConTurnosActivos { get; } = [];

    public int SaveChangesCount { get; private set; }

    public Task<(IReadOnlyList<Paciente> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        IEnumerable<Paciente> query = _pacientes.Where(p => p.DeletedAt is null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p =>
                $"{p.Nombre} {p.Apellido}".Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = query.OrderBy(p => p.Id).ToList();
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Paciente>, int)>((items, ordered.Count));
    }

    public Task<Paciente?> GetByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_pacientes.FirstOrDefault(p => p.Id == id && p.DeletedAt is null));

    public Task AddAsync(Paciente paciente, CancellationToken ct)
    {
        paciente.Id = _nextId++;
        _pacientes.Add(paciente);
        return Task.CompletedTask;
    }

    public void Update(Paciente paciente)
    {
    }

    public Task<bool> TieneTurnosActivosAsync(int pacienteId, CancellationToken ct) =>
        Task.FromResult(ConTurnosActivos.Contains(pacienteId));

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
