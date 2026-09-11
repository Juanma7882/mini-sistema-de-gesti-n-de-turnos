using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Api.Auth;
using Turnos.Application.Common;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Api.Controllers;

/// <summary>Turnos. Lectura y <c>PATCH estado</c> para cualquier autenticado —
/// el alcance por rol (Profesional solo ve/toca los suyos, 404 en ajeno) y la
/// máquina de estados los aplica <see cref="TurnoService"/> vía
/// <c>ICurrentUser</c>, no este controlador. Crear/editar datos: solo Admin.
/// No hay <c>DELETE</c>: cancelar es <c>PATCH estado = Cancelado</c>.</summary>
[Authorize]
public sealed class TurnosController(TurnoService service) : ApiControllerBase
{
    // page/pageSize como primitivos sueltos, no PageRequest directo: ver nota
    // de PacientesController/ProfesionalesController (choque de nombre con
    // PageRequest.Page en el model binder).
    [HttpGet]
    public async Task<ActionResult<PagedResult<TurnoDto>>> Listar(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] EstadoTurno? estado,
        [FromQuery] int? pacienteId,
        [FromQuery] int? profesionalId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.PageSizePorDefecto,
        CancellationToken ct = default)
    {
        var filtro = new TurnoFiltro
        {
            Desde = desde,
            Hasta = hasta,
            Estado = estado,
            PacienteId = pacienteId,
            ProfesionalId = profesionalId,
        };

        var resultado = await service.ListarAsync(
            filtro, new PageRequest { Page = page, PageSize = pageSize }, ct);

        return Ok(resultado);
    }

    /// <summary>404 si no existe o (Profesional) no es suyo — mismo código para
    /// ambos casos, no se revela que el turno existe.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurnoDto>> Obtener(int id, CancellationToken ct)
    {
        var turno = await service.ObtenerAsync(id, ct);

        return Ok(turno);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<TurnoDto>> Crear(TurnoRequest request, CancellationToken ct)
    {
        var turno = await service.CrearAsync(request, ct);

        return CreatedAtAction(nameof(Obtener), new { id = turno.Id }, turno);
    }

    /// <summary>Datos del turno (paciente/profesional/inicio/notas), nunca el
    /// estado. 409 si el turno ya está <c>Cancelado</c>/<c>Atendido</c> o si el
    /// nuevo slot choca.</summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TurnoDto>> Editar(int id, TurnoRequest request, CancellationToken ct)
    {
        var turno = await service.EditarAsync(id, request, ct);

        return Ok(turno);
    }

    /// <summary>Admin dispara cualquier transición legal; Profesional solo las
    /// suyas (sobre sus propios turnos). 409 en transición ilegal, 404 en turno
    /// ajeno/inexistente.</summary>
    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<TurnoDto>> CambiarEstado(
        int id, CambiarEstadoRequest request, CancellationToken ct)
    {
        var turno = await service.CambiarEstadoAsync(id, request.Estado, ct);

        return Ok(turno);
    }
}
