using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Turnos.Api.Auth;
using Turnos.Application.Auth;

namespace Turnos.Api.Controllers;

/// <summary>Login único para Admin y Profesional (la diferencia sale del
/// <c>Rol</c> del usuario, no de la ruta), refresh con rotación revoke-on-use
/// vía cookie <c>httpOnly</c>, logout y el usuario autenticado. Ver README
/// §Autenticación y permisos.</summary>
public sealed class AuthController(AuthService authService) : ApiControllerBase
{
    /// <summary>Sin <c>[Authorize]</c>: anónimo por defecto (no hay fallback
    /// policy global). 401 si las credenciales son inválidas (lo lanza
    /// <see cref="AuthService"/>, lo traduce <c>ExceptionHandler</c>). 429 si
    /// se superan los intentos por IP de la policy <c>login</c> (rate
    /// limiter, ver <c>Api/DependencyInjection.AddLoginRateLimiting</c>).</summary>
    [EnableRateLimiting(Turnos.Api.DependencyInjection.LoginRateLimitPolicy)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var session = await authService.LoginAsync(request, ct);
        RefreshCookie.Append(Response, session.RefreshToken, session.RefreshTokenExpiresAt);

        return Ok(session.Result);
    }

    /// <summary>Lee el refresh token de la cookie <c>rt</c>, no del body. 401 si
    /// falta, expiró o ya fue usado (revoke-on-use).</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponse>> Refresh(CancellationToken ct)
    {
        var rawToken = RefreshCookie.Read(Request);
        var session = await authService.RefreshAsync(rawToken, ct);
        RefreshCookie.Append(Response, session.RefreshToken, session.RefreshTokenExpiresAt);

        return Ok(new RefreshResponse(session.Token));
    }

    /// <summary>Revoca el refresh token actual (idempotente: no falla si la
    /// cookie no vino o ya estaba revocada) y borra la cookie.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var rawToken = RefreshCookie.Read(Request);
        await authService.LogoutAsync(rawToken, ct);
        RefreshCookie.Delete(Response);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeDto>> Me(CancellationToken ct)
    {
        var me = await authService.MeAsync(ct);

        return Ok(me);
    }
}
