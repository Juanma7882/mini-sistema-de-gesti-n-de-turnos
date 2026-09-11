using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Api.Auth;
using Turnos.Application.Common;
using Turnos.Application.Pacientes;

namespace Turnos.Api.Controllers;

/// <summary>ABM de pacientes. Solo Admin (lectura y escritura) — ver README
/// §Pacientes.</summary>
[Authorize(Roles = Roles.Admin)]
public sealed class PacientesController(PacienteService service) : ApiControllerBase
{
    // page/pageSize se bindean como primitivos (no como PageRequest directo):
    // el nombre del parámetro "page" colisiona con la propiedad PageRequest.Page
    // y el model binder de [FromQuery] no resuelve el query string plano.
    [HttpGet]
    public async Task<ActionResult<PagedResult<PacienteDto>>> Listar(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.PageSizePorDefecto,
        CancellationToken ct = default)
    {
        var resultado = await service.ListarAsync(
            search, new PageRequest { Page = page, PageSize = pageSize }, ct);

        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Obtener(int id, CancellationToken ct)
    {
        var paciente = await service.ObtenerAsync(id, ct);

        return Ok(paciente);
    }

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Crear(PacienteRequest request, CancellationToken ct)
    {
        var paciente = await service.CrearAsync(request, ct);

        return CreatedAtAction(nameof(Obtener), new { id = paciente.Id }, paciente);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Editar(int id, PacienteRequest request, CancellationToken ct)
    {
        var paciente = await service.EditarAsync(id, request, ct);

        return Ok(paciente);
    }

    /// <summary>Baja lógica. 409 si el paciente tiene turnos activos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await service.BajaAsync(id, ct);

        return NoContent();
    }
}
