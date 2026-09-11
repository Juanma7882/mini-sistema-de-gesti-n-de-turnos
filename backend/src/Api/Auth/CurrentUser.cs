using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Api.Auth;

/// <summary>Implementación de <see cref="ICurrentUser"/> sobre los claims del
/// <see cref="ClaimsPrincipal"/> autenticado. Depende de que <c>AddJwtBearer</c>
/// (ver <c>DependencyInjection.AddApi</c>) deje los claims <c>sub</c>/<c>role</c>
/// sin remapear (<c>MapInboundClaims = false</c>).</summary>
internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int UsuarioId => int.Parse(RequireClaim(JwtRegisteredClaimNames.Sub), CultureInfo.InvariantCulture);

    public Rol Rol => Enum.Parse<Rol>(RequireClaim(ClaimTypes.Role));

    /// <summary>Claim solo presente para usuarios con rol Profesional.</summary>
    public int? ProfesionalId => Principal?.FindFirst("profesionalId")?.Value is { } value
        ? int.Parse(value, CultureInfo.InvariantCulture)
        : null;

    private string RequireClaim(string type) =>
        Principal?.FindFirst(type)?.Value
            ?? throw new InvalidOperationException(
                $"No hay usuario autenticado o falta el claim '{type}'.");
}
