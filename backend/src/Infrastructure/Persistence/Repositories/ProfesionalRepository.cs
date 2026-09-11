using Microsoft.EntityFrameworkCore;
using Turnos.Application.Abstractions;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;

namespace Turnos.Infrastructure.Persistence.Repositories;

internal sealed class ProfesionalRepository(AppDbContext db) : IProfesionalRepository
{
    public async Task<(IReadOnlyList<Profesional> Items, int Total)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<Profesional> query = db.Profesionales.AsNoTracking().Include(p => p.Usuario);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                (p.Usuario.Nombre + " " + p.Usuario.Apellido).ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Usuario.Apellido)
            .ThenBy(p => p.Usuario.Nombre)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Profesional?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Profesionales.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Profesional profesional, CancellationToken ct) =>
        await db.Profesionales.AddAsync(profesional, ct);

    public void Update(Profesional profesional) => db.Profesionales.Update(profesional);

    public Task<bool> TieneTurnosActivosAsync(int profesionalId, CancellationToken ct) =>
        db.Turnos.AnyAsync(
            t => t.ProfesionalId == profesionalId
                 && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado),
            ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
