using Turnos.Application.Abstractions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Profesionales;

namespace Turnos.Application.Profesionales;

/// <summary>Casos de uso del ABM de profesionales. En la Api, lectura para
/// cualquier autenticado y escritura solo Admin.</summary>
public sealed class ProfesionalService(IProfesionalRepository repository, IClock clock)
{
    public async Task<PagedResult<ProfesionalDto>> ListarAsync(
        string? search, PageRequest page, CancellationToken ct)
    {
        var p = page.Normalizado();
        var (items, total) = await repository.GetPagedAsync(search, p.Page, p.PageSize, ct);

        return new PagedResult<ProfesionalDto>
        {
            Items = items.Select(ProfesionalMapper.ToDto).ToList(),
            Total = total,
            Page = p.Page,
            PageSize = p.PageSize,
        };
    }

    public async Task<ProfesionalDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        return ProfesionalMapper.ToDto(profesional);
    }

    public async Task<ProfesionalDto> CrearAsync(ProfesionalRequest request, CancellationToken ct)
    {
        var profesional = new Profesional
        {
            Nombre = TextoNormalizer.NombrePropio(request.Nombre),
            Apellido = TextoNormalizer.NombrePropio(request.Apellido),
            Especialidad = request.Especialidad.Trim(),
            CreatedAt = clock.UtcNow,
        };

        await repository.AddAsync(profesional, ct);
        await repository.SaveChangesAsync(ct);

        return ProfesionalMapper.ToDto(profesional);
    }

    public async Task<ProfesionalDto> EditarAsync(
        int id, ProfesionalRequest request, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        profesional.Nombre = TextoNormalizer.NombrePropio(request.Nombre);
        profesional.Apellido = TextoNormalizer.NombrePropio(request.Apellido);
        profesional.Especialidad = request.Especialidad.Trim();

        repository.Update(profesional);
        await repository.SaveChangesAsync(ct);

        return ProfesionalMapper.ToDto(profesional);
    }

    /// <summary>Baja lógica. <c>409</c> si el profesional tiene turnos activos
    /// (<c>Pendiente</c>/<c>Confirmado</c>). Idempotente si ya estaba dado de baja.</summary>
    public async Task BajaAsync(int id, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        if (profesional.DeletedAt is not null)
        {
            return;
        }

        if (await repository.TieneTurnosActivosAsync(id, ct))
        {
            throw new ConflictException(
                "El profesional tiene turnos activos y no puede darse de baja.");
        }

        profesional.DeletedAt = clock.UtcNow;
        repository.Update(profesional);
        await repository.SaveChangesAsync(ct);
    }
}
