using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Auth;
using Turnos.Domain.Usuarios;

namespace Turnos.Api.Tests.Auth;

[Collection(ApiCollection.Name)]
public sealed class AuthTests(IntegrationTestFactory factory)
{
    [Fact]
    public async Task Login_CredencialesValidas_DevuelveTokenYCookieSegura()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = LoginHelper.AdminEmail, Password = LoginHelper.AdminPassword },
            ApiJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResultDto>(ApiJson.Options);
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Role.Should().Be(Rol.Admin);
        body.User.ProfesionalId.Should().BeNull();

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var rt = cookies!.Single(c => c.StartsWith("rt=", StringComparison.Ordinal));
        // Atributos de §7: httpOnly + Secure + SameSite=None (cross-site
        // front/back) + Path=/api/auth (login/refresh/logout, no solo /refresh).
        rt.Should().Contain("httponly");
        rt.Should().Contain("secure");
        rt.Should().Contain("samesite=none");
        rt.Should().Contain("path=/api/auth");
    }

    [Fact]
    public async Task Login_CredencialesInvalidas_Devuelve401()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = LoginHelper.AdminEmail, Password = "password-incorrecta" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ConCookieValida_RotaLaCookieYElTokenViejoQuedaRevocado()
    {
        using var sesion = await LoginHelper.LoginAsync(factory, LoginHelper.AdminEmail, LoginHelper.AdminPassword);
        using var anonimo = factory.CreateClient();

        var primeraRespuesta = await anonimo.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithRefreshCookie(sesion.RefreshToken));
        primeraRespuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var nuevoRawToken = LoginHelper.ExtraerCookie(primeraRespuesta, "rt");
        nuevoRawToken.Should().NotBeNullOrEmpty();
        nuevoRawToken.Should().NotBe(sesion.RefreshToken);

        // Reusar el token viejo (ya rotado por el refresh anterior) -> 401.
        var segundaRespuesta = await anonimo.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithRefreshCookie(sesion.RefreshToken));
        segundaRespuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevocaElRefreshToken_YaNoSirveParaRefrescar()
    {
        using var sesion = await LoginHelper.LoginAsync(factory, LoginHelper.AdminEmail, LoginHelper.AdminPassword);

        var logoutResponse = await sesion.Http.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout").WithRefreshCookie(sesion.RefreshToken));
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var anonimo = factory.CreateClient();
        var refreshResponse = await anonimo.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithRefreshCookie(sesion.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_SinToken_Devuelve401()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
