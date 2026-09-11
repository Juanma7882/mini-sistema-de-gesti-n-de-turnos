using Microsoft.AspNetCore.Mvc;

namespace Turnos.Api.Controllers;

/// <summary>Base de todos los controladores: fuerza el prefijo global
/// <c>/api</c> (§7–§9 lo cuelgan de acá) y el comportamiento de
/// <c>[ApiController]</c> — el 400 automático de <c>[ApiController]</c> está
/// desactivado (<c>AddApi</c>, <c>SuppressModelStateInvalidFilter = true</c>);
/// lo dispara <see cref="Filters.ValidationFilter"/> en su lugar.</summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase;
