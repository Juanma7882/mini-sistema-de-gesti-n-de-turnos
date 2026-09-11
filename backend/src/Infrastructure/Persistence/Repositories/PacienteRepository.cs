using Microsoft.EntityFrameworkCore;
using Turnos.Application.Abstractions;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Turnos;

namespace Turnos.Infrastructure.Persistence.Repositories;

internal sealed class PacienteRepository(AppDbContext db) : IPacienteRepository
{
    public async Task<(IReadOnlyList<Paciente> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                (p.Nombre + " " + p.Apellido).ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Paciente?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Pacientes.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Paciente paciente, CancellationToken ct) =>
        await db.Pacientes.AddAsync(paciente, ct);

    public void Update(Paciente paciente) => db.Pacientes.Update(paciente);

    public Task<bool> TieneTurnosActivosAsync(int pacienteId, CancellationToken ct) =>
        db.Turnos.AnyAsync(
            t => t.PacienteId == pacienteId
                 && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado),
            ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
