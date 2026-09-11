using FluentAssertions;
using Turnos.Application.Auth;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Tests.Fakes;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests.Auth;

public class AuthServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly FakeClock _clock = new();
    private readonly FakeJwtTokenService _jwt = new();
    private readonly FakeRefreshTokenService _refresh;

    public AuthServiceTests() => _refresh = new FakeRefreshTokenService(_clock);

    private AuthService Crear(FakeCurrentUser currentUser, params Usuario[] usuarios) =>
        new(new FakeUsuarioRepository(usuarios), new FakePasswordHasher(), _jwt, _refresh, currentUser);

    [Fact]
    public async Task LoginAsync_CredencialesValidas_DevuelveTokenYUsuario()
    {
        var admin = TestData.Admin(id: 1, email: "admin@clinica.test");
        var sut = Crear(FakeCurrentUser.Admin(), admin);

        var sesion = await sut.LoginAsync(
            new LoginRequest { Email = "admin@clinica.test", Password = "secret" }, Ct);

        sesion.Result.Token.Should().NotBeNullOrWhiteSpace();
        sesion.Result.User.Id.Should().Be(1);
        sesion.Result.User.Role.Should().Be(Rol.Admin);
        sesion.RefreshToken.Should().NotBeNullOrWhiteSpace();
        sesion.RefreshTokenExpiresAt.Should().Be(_clock.UtcNow.AddDays(7));
    }

    [Fact]
    public async Task LoginAsync_PasswordIncorrecta_LanzaUnauthorized()
    {
        var sut = Crear(FakeCurrentUser.Admin(), TestData.Admin(email: "admin@clinica.test"));

        await sut.Invoking(s => s.LoginAsync(
                new LoginRequest { Email = "admin@clinica.test", Password = "mala" }, Ct))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_EmailInexistente_LanzaUnauthorized()
    {
        var sut = Crear(FakeCurrentUser.Admin(), TestData.Admin(email: "admin@clinica.test"));

        await sut.Invoking(s => s.LoginAsync(
                new LoginRequest { Email = "otro@clinica.test", Password = "secret" }, Ct))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshAsync_RotaElTokenYRevocaElAnterior()
    {
        var sut = Crear(FakeCurrentUser.Admin(), TestData.Admin(id: 1, email: "admin@clinica.test"));
        var login = await sut.LoginAsync(
            new LoginRequest { Email = "admin@clinica.test", Password = "secret" }, Ct);

        var rotado = await sut.RefreshAsync(login.RefreshToken, Ct);

        rotado.RefreshToken.Should().NotBe(login.RefreshToken);
        rotado.Token.Should().NotBeNullOrWhiteSpace();
        _refresh.EstaRevocado(login.RefreshToken).Should().BeTrue();

        await sut.Invoking(s => s.RefreshAsync(login.RefreshToken, Ct))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshAsync_SinToken_LanzaUnauthorized(string? token)
    {
        var sut = Crear(FakeCurrentUser.Admin(), TestData.Admin());

        await sut.Invoking(s => s.RefreshAsync(token, Ct))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LogoutAsync_SinCookie_NoFalla()
    {
        var sut = Crear(FakeCurrentUser.Admin(), TestData.Admin());

        await sut.Invoking(s => s.LogoutAsync(null, Ct)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task MeAsync_DevuelveElUsuarioAutenticado()
    {
        var profesional = TestData.Profesional(id: 7, profesionalId: 10, email: "dra@clinica.test");
        var sut = Crear(FakeCurrentUser.Profesional(profesionalId: 10, usuarioId: 7), profesional);

        var me = await sut.MeAsync(Ct);

        me.Id.Should().Be(7);
        me.Role.Should().Be(Rol.Profesional);
        me.ProfesionalId.Should().Be(10);
    }
}
