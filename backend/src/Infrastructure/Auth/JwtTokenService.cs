using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Infrastructure.Auth;

/// <summary>Emite el access token JWT HS256 con los claims <c>sub</c>,
/// <c>email</c>, <c>role</c> y (solo Profesional) <c>profesionalId</c>.</summary>
internal sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public string CreateAccessToken(Usuario usuario)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
        };

        if (usuario.Profesional is { } profesional)
        {
            claims.Add(new Claim(
                "profesionalId",
                profesional.Id.ToString(CultureInfo.InvariantCulture)));
        }

        var now = clock.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.AccessMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
