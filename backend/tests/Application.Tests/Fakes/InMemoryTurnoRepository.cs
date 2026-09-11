using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Turnos;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Repositorio de turnos en memoria. Hidrata <c>Paciente</c>/<c>Profesional</c>
/// en las lecturas (como el <c>Include</c> real) y sabe simular la carrera del
/// índice único parcial vía <see cref="ForzarChoqueEnSave"/>.</summary>
public sealed class InMemoryTurnoRepository : ITurnoRepository
{
    private readonly List<Turno> _turnos;
    private readonly IReadOnlyList<Paciente> _pacientes;
    private readonly IReadOnlyList<Profesional> _profesionales;
    private int _nextId;

    public InMemoryTurnoRepository(
        IEnumerable<Paciente> pacientes,
        IEnumerable<Profesional> profesionales,
        params Turno[] seed)
    {
        _pacientes = pacientes.ToList();
        _profesionales = profesionales.ToList();
        _turnos = seed.ToList();
        _nextId = _turnos.Count == 0 ? 1 : _turnos.Max(t => t.Id) + 1;
    }

    public bool ForzarChoqueEnSave { get; set; }

    public int SaveChangesCount { get; private set; }

    public Task<(IReadOnlyList<Turno> Items, int Total)> GetPagedAsync(
        TurnoFiltro filtro, int page, int pageSize, CancellationToken ct)
    {
        var query = _turnos.AsEnumerable();

        if (filtro.Desde is { } desde)
        {
            query = query.Where(t => t.Inicio >= desde);
        }

        if (filtro.Hasta is { } hasta)
        {
            query = query.Where(t => t.Inicio <= hasta);
        }

        if (filtro.Estado is { } estado)
        {
            query = query.Where(t => t.Estado == estado);
        }

        if (filtro.PacienteId is { } pacienteId)
        {
            query = query.Where(t => t.PacienteId == pacienteId);
        }

        if (filtro.ProfesionalId is { } profesionalId)
        {
            query = query.Where(t => t.ProfesionalId == profesionalId);
        }

        var ordered = query.OrderBy(t => t.Inicio).ToList();
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Hidratar)
            .ToList();

        return Task.FromResult<(IReadOnlyList<Turno>, int)>((items, ordered.Count));
    }

    public Task<Turno?> GetByIdAsync(int id, CancellationToken ct)
    {
        var turno = _turnos.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(turno is null ? null : Hidratar(turno));
    }

    public Task AddAsync(Turno turno, CancellationToken ct)
    {
        turno.Id = _nextId++;
        _turnos.Add(turno);
        return Task.CompletedTask;
    }

    public void Update(Turno turno)
    {
    }

    public Task<bool> ExisteSlotAsync(
        int profesionalId, DateTime inicio, int? excluirTurnoId, CancellationToken ct) =>
        Task.FromResult(_turnos.Any(t =>
            t.ProfesionalId == profesionalId
            && t.Inicio == inicio
            && t.Estado != EstadoTurno.Cancelado
            && t.Id != (excluirTurnoId ?? 0)));

    public Task<bool> TienePacienteActivoConProfesionalAsync(
        int profesionalId, int pacienteId, int? excluirTurnoId, CancellationToken ct) =>
        Task.FromResult(_turnos.Any(t =>
            t.ProfesionalId == profesionalId
            && t.PacienteId == pacienteId
            && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado)
            && t.Id != (excluirTurnoId ?? 0)));

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCount++;

        if (ForzarChoqueEnSave)
        {
            throw new ConflictException("El profesional ya tiene un turno en ese horario.");
        }

        return Task.CompletedTask;
    }

    private Turno Hidratar(Turno turno)
    {
        turno.Paciente = _pacientes.First(p => p.Id == turno.PacienteId);
        turno.Profesional = _profesionales.First(p => p.Id == turno.ProfesionalId);
        return turno;
    }
}
