using Turnos.Application.Abstractions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Pacientes;

namespace Turnos.Application.Pacientes;

/// <summary>Casos de uso del ABM de pacientes (solo Admin en la Api).</summary>
public sealed class PacienteService(IPacienteRepository repository, IClock clock)
{
    public async Task<PagedResult<PacienteDto>> ListarAsync(
        string? search, PageRequest page, CancellationToken ct)
    {
        var p = page.Normalizado();
        var (items, total) = await repository.GetPagedAsync(search, p.Page, p.PageSize, ct);

        return new PagedResult<PacienteDto>
        {
            Items = items.Select(PacienteMapper.ToDto).ToList(),
            Total = total,
            Page = p.Page,
            PageSize = p.PageSize,
        };
    }

    public async Task<PacienteDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var paciente = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Paciente", id);

        return PacienteMapper.ToDto(paciente);
    }

    public async Task<PacienteDto> CrearAsync(PacienteRequest request, CancellationToken ct)
    {
        var paciente = new Paciente
        {
            Nombre = TextoNormalizer.NombrePropio(request.Nombre),
            Apellido = TextoNormalizer.NombrePropio(request.Apellido),
            Telefono = request.Telefono.Trim(),
            ObraSocial = TextoNormalizer.TextoLibre(request.ObraSocial),
            CreatedAt = clock.UtcNow,
        };

        await repository.AddAsync(paciente, ct);
        await repository.SaveChangesAsync(ct);

        return PacienteMapper.ToDto(paciente);
    }

    public async Task<PacienteDto> EditarAsync(int id, PacienteRequest request, CancellationToken ct)
    {
        var paciente = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Paciente", id);

        paciente.Nombre = TextoNormalizer.NombrePropio(request.Nombre);
        paciente.Apellido = TextoNormalizer.NombrePropio(request.Apellido);
        paciente.Telefono = request.Telefono.Trim();
        paciente.ObraSocial = TextoNormalizer.TextoLibre(request.ObraSocial);

        repository.Update(paciente);
        await repository.SaveChangesAsync(ct);

        return PacienteMapper.ToDto(paciente);
    }

    /// <summary>Baja lógica. <c>409</c> si el paciente tiene turnos activos
    /// (<c>Pendiente</c>/<c>Confirmado</c>). Idempotente si ya estaba dado de baja.</summary>
    public async Task BajaAsync(int id, CancellationToken ct)
    {
        var paciente = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Paciente", id);

        if (paciente.DeletedAt is not null)
        {
            return;
        }

        if (await repository.TieneTurnosActivosAsync(id, ct))
        {
            throw new ConflictException(
                "El paciente tiene turnos activos y no puede darse de baja.");
        }

        paciente.DeletedAt = clock.UtcNow;
        repository.Update(paciente);
        await repository.SaveChangesAsync(ct);
    }
}
