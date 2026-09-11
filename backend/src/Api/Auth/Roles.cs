using Turnos.Domain.Usuarios;

namespace Turnos.Api.Auth;

/// <summary>Nombres de rol para <c>[Authorize(Roles = ...)]</c>, evita strings
/// sueltos en los controladores de §7–§9. Coinciden con <see cref="Rol"/> porque
/// <c>JwtTokenService</c> emite el claim de rol con <c>Rol.ToString()</c>.</summary>
internal static class Roles
{
    public const string Admin = nameof(Rol.Admin);

    public const string Profesional = nameof(Rol.Profesional);
}
