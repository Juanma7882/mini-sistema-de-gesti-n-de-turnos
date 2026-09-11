using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Infrastructure.Persistence.Repositories;

internal sealed class TurnoRepository(AppDbContext db) : ITurnoRepository
{
    public async Task<(IReadOnlyList<Turno> Items, int Total)> GetPagedAsync(
        TurnoFiltro filtro, int page, int pageSize, CancellationToken ct)
    {
        var query = FiltradoBase(filtro).AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Inicio)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Turno?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Profesional).ThenInclude(p => p.Usuario)
            // El paciente/profesional puede haberse dado de baja después de un
            // turno ya cerrado; igual queremos mostrarlo en el detalle/listado.
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Turno turno, CancellationToken ct) =>
        await db.Turnos.AddAsync(turno, ct);

    public void Update(Turno turno) => db.Turnos.Update(turno);

    public Task<bool> ExisteSlotAsync(
        int profesionalId, DateTime inicio, int? excluirTurnoId, CancellationToken ct) =>
        db.Turnos.AnyAsync(
            t => t.ProfesionalId == profesionalId
                 && t.Inicio == inicio
                 && t.Estado != EstadoTurno.Cancelado
                 && (excluirTurnoId == null || t.Id != excluirTurnoId),
            ct);

    public Task<bool> TienePacienteActivoConProfesionalAsync(
        int profesionalId, int pacienteId, int? excluirTurnoId, CancellationToken ct) =>
        db.Turnos.AnyAsync(
            t => t.ProfesionalId == profesionalId
                 && t.PacienteId == pacienteId
                 && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado)
                 && (excluirTurnoId == null || t.Id != excluirTurnoId),
            ct);

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsViolacionDeIndiceUnico(ex))
        {
            // Carrera entre el pre-chequeo del servicio y el insert: lo corta
            // alguno de los dos índices únicos parciales de Turnos.
            throw new ConflictException(
                "El turno choca con otro del profesional: mismo horario, "
                + "o el paciente ya tiene un turno activo con él.");
        }
    }

    private IQueryable<Turno> FiltradoBase(TurnoFiltro filtro)
    {
        var query = db.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Profesional).ThenInclude(p => p.Usuario)
            .IgnoreQueryFilters()
            .AsQueryable();

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

        return query;
    }

    private static bool EsViolacionDeIndiceUnico(DbUpdateException ex) =>
        ex.InnerException is SqliteException { SqliteErrorCode: 19 } inner
        && inner.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
}
