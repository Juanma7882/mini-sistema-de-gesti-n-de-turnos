using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Api.Auth;
using Turnos.Application.Common;
using Turnos.Application.Profesionales;

namespace Turnos.Api.Controllers;

/// <summary>ABM de profesionales. Lectura para cualquier autenticado (Admin o
/// Profesional); escritura solo Admin — ver README §Profesionales. El
/// <c>[Authorize]</c> de clase exige estar autenticado; el <c>[Authorize(Roles=…)]</c>
/// de cada acción de escritura se combina con ese (AND), no lo reemplaza.</summary>
[Authorize]
public sealed class ProfesionalesController(ProfesionalService service) : ApiControllerBase
{
    // page/pageSize se bindean como primitivos (no como PageRequest directo):
    // el nombre del parámetro "page" colisiona con la propiedad PageRequest.Page
    // y el model binder de [FromQuery] no resuelve el query string plano.
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProfesionalDto>>> Listar(
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
    public async Task<ActionResult<ProfesionalDto>> Obtener(int id, CancellationToken ct)
    {
        var profesional = await service.ObtenerAsync(id, ct);

        return Ok(profesional);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ProfesionalDto>> Crear(CrearProfesionalRequest request, CancellationToken ct)
    {
        var profesional = await service.CrearAsync(request, ct);

        return CreatedAtAction(nameof(Obtener), new { id = profesional.Id }, profesional);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProfesionalDto>> Editar(int id, ProfesionalRequest request, CancellationToken ct)
    {
        var profesional = await service.EditarAsync(id, request, ct);

        return Ok(profesional);
    }

    /// <summary>Baja lógica. 409 si el profesional tiene turnos activos.</summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await service.BajaAsync(id, ct);

        return NoContent();
    }
}
