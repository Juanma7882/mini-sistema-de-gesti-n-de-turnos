using Turnos.Application.Abstractions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Turnos;

/// <summary>
/// Casos de uso de turnos. Aplica el alcance por rol (el Profesional solo ve y
/// toca sus turnos; acceder a uno ajeno da <c>404</c>, no <c>403</c>), el
/// anti doble-turno y la máquina de estados.
/// </summary>
public sealed class TurnoService(
    ITurnoRepository turnos,
    IPacienteRepository pacientes,
    IProfesionalRepository profesionales,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task<PagedResult<TurnoDto>> ListarAsync(
        TurnoFiltro filtro, PageRequest page, CancellationToken ct)
    {
        var p = page.Normalizado();
        var (items, total) = await turnos.GetPagedAsync(
            AplicarAlcancePorRol(filtro), p.Page, p.PageSize, ct);

        return new PagedResult<TurnoDto>
        {
            Items = items.Select(TurnoMapper.ToDto).ToList(),
            Total = total,
            Page = p.Page,
            PageSize = p.PageSize,
        };
    }

    public async Task<TurnoDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var turno = await turnos.GetByIdAsync(id, ct);
        if (turno is null || !PuedeVer(turno))
        {
            throw NotFoundException.Para("Turno", id);
        }

        return TurnoMapper.ToDto(turno);
    }

    public async Task<TurnoDto> CrearAsync(TurnoRequest request, CancellationToken ct)
    {
        await GarantizarPacienteActivoAsync(request.PacienteId, ct);
        await GarantizarProfesionalActivoAsync(request.ProfesionalId, ct);
        await GarantizarSlotLibreAsync(request.ProfesionalId, request.Inicio, excluirTurnoId: null, ct);
        await GarantizarPacienteSinTurnoActivoAsync(
            request.ProfesionalId, request.PacienteId, excluirTurnoId: null, ct);

        var ahora = clock.UtcNow;
        var turno = new Turno
        {
            PacienteId = request.PacienteId,
            ProfesionalId = request.ProfesionalId,
            Inicio = request.Inicio,
            Notas = NormalizarNotas(request.Notas),
            Estado = EstadoTurno.Pendiente,
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };

        await turnos.AddAsync(turno, ct);
        // Si otra request tomó el slot entremedio, el repo traduce la violación
        // del índice único parcial a ConflictException.
        await turnos.SaveChangesAsync(ct);

        return await RecargarDtoAsync(turno.Id, ct);
    }

    public async Task<TurnoDto> EditarAsync(int id, TurnoRequest request, CancellationToken ct)
    {
        var turno = await turnos.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Turno", id);

        if (turno.Estado is EstadoTurno.Cancelado or EstadoTurno.Atendido)
        {
            throw new ConflictException(
                $"El turno {id} está {turno.Estado} y no puede modificarse; hay que crear uno nuevo.");
        }

        await GarantizarPacienteActivoAsync(request.PacienteId, ct);
        await GarantizarProfesionalActivoAsync(request.ProfesionalId, ct);
        await GarantizarSlotLibreAsync(request.ProfesionalId, request.Inicio, excluirTurnoId: id, ct);
        await GarantizarPacienteSinTurnoActivoAsync(
            request.ProfesionalId, request.PacienteId, excluirTurnoId: id, ct);

        turno.PacienteId = request.PacienteId;
        turno.ProfesionalId = request.ProfesionalId;
        turno.Inicio = request.Inicio;
        turno.Notas = NormalizarNotas(request.Notas);
        turno.UpdatedAt = clock.UtcNow;

        turnos.Update(turno);
        await turnos.SaveChangesAsync(ct);

        return await RecargarDtoAsync(id, ct);
    }

    /// <summary>Cambia el estado si la transición es legal
    /// (<see cref="MaquinaEstados"/>). Ambos roles pueden disparar cualquier
    /// flecha legal sobre sus propios turnos; el rol solo limita <em>sobre qué
    /// turnos</em>. Transición ilegal → <c>409</c>.</summary>
    public async Task<TurnoDto> CambiarEstadoAsync(
        int id, EstadoTurno nuevoEstado, CancellationToken ct)
    {
        var turno = await turnos.GetByIdAsync(id, ct);
        if (turno is null || !PuedeVer(turno))
        {
            throw NotFoundException.Para("Turno", id);
        }

        if (!MaquinaEstados.EsTransicionValida(turno.Estado, nuevoEstado))
        {
            throw new ConflictException(
                $"No se puede pasar un turno de {turno.Estado} a {nuevoEstado}.");
        }

        turno.Estado = nuevoEstado;
        turno.UpdatedAt = clock.UtcNow;

        turnos.Update(turno);
        await turnos.SaveChangesAsync(ct);

        return TurnoMapper.ToDto(turno);
    }

    // ---- helpers ----

    private TurnoFiltro AplicarAlcancePorRol(TurnoFiltro filtro) =>
        currentUser.Rol == Rol.Profesional
            ? filtro with { ProfesionalId = currentUser.ProfesionalId }
            : filtro;

    private bool PuedeVer(Turno turno) =>
        currentUser.Rol != Rol.Profesional
        || turno.ProfesionalId == currentUser.ProfesionalId;

    private async Task GarantizarPacienteActivoAsync(int pacienteId, CancellationToken ct)
    {
        var paciente = await pacientes.GetByIdAsync(pacienteId, ct);
        if (paciente is null || paciente.DeletedAt is not null)
        {
            throw NotFoundException.Para("Paciente", pacienteId);
        }
    }

    private async Task GarantizarProfesionalActivoAsync(int profesionalId, CancellationToken ct)
    {
        var profesional = await profesionales.GetByIdAsync(profesionalId, ct);
        if (profesional is null || profesional.DeletedAt is not null)
        {
            throw NotFoundException.Para("Profesional", profesionalId);
        }
    }

    private async Task GarantizarSlotLibreAsync(
        int profesionalId, DateTime inicio, int? excluirTurnoId, CancellationToken ct)
    {
        if (await turnos.ExisteSlotAsync(profesionalId, inicio, excluirTurnoId, ct))
        {
            throw new ConflictException(
                $"El profesional ya tiene un turno el {inicio:yyyy-MM-dd} a las {inicio:HH:mm}.");
        }
    }

    private async Task GarantizarPacienteSinTurnoActivoAsync(
        int profesionalId, int pacienteId, int? excluirTurnoId, CancellationToken ct)
    {
        if (await turnos.TienePacienteActivoConProfesionalAsync(
                profesionalId, pacienteId, excluirTurnoId, ct))
        {
            throw new ConflictException(
                "El paciente ya tiene un turno activo con ese profesional.");
        }
    }

    private async Task<TurnoDto> RecargarDtoAsync(int id, CancellationToken ct)
    {
        var turno = await turnos.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Turno", id);

        return TurnoMapper.ToDto(turno);
    }

    private static string? NormalizarNotas(string? notas) =>
        string.IsNullOrWhiteSpace(notas) ? null : notas.Trim();
}
