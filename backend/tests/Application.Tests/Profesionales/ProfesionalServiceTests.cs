using FluentAssertions;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Profesionales;
using Turnos.Application.Tests.Fakes;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests.Profesionales;

public class ProfesionalServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeClock _clock = new();

    private ProfesionalService Crear(
        InMemoryProfesionalRepository repository, FakeUsuarioRepository? usuarios = null) =>
        new(repository, usuarios ?? new FakeUsuarioRepository(), new FakePasswordHasher(), _clock);

    private static CrearProfesionalRequest RequestValido(string email = "laura.gomez@clinica.test") => new()
    {
        Nombre = "laURA",
        Apellido = "GÓMEZ",
        Especialidad = " Pediatría ",
        Email = email,
        Password = "Password123*",
    };

    [Fact]
    public async Task CrearAsync_NormalizaNombreYApellido()
    {
        var repo = new InMemoryProfesionalRepository();
        var sut = Crear(repo);

        var dto = await sut.CrearAsync(RequestValido(), Ct);

        dto.Nombre.Should().Be("Laura");
        dto.Apellido.Should().Be("Gómez");
        dto.Especialidad.Should().Be("Pediatría");
        dto.CreatedAt.Should().Be(_clock.UtcNow);
    }

    [Fact]
    public async Task CrearAsync_CreaUsuarioAsociadoConRolProfesional()
    {
        var repo = new InMemoryProfesionalRepository();
        var usuarios = new FakeUsuarioRepository();
        var sut = Crear(repo, usuarios);

        var dto = await sut.CrearAsync(RequestValido(), Ct);

        var usuario = usuarios.Usuarios.Should().ContainSingle().Subject;
        usuario.Email.Should().Be("laura.gomez@clinica.test");
        usuario.Rol.Should().Be(Rol.Profesional);
        usuario.Profesional.Should().NotBeNull();
        usuario.Profesional!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task CrearAsync_EmailYaRegistrado_LanzaConflict()
    {
        var existente = new Usuario { Email = "laura.gomez@clinica.test", Rol = Rol.Admin };
        var repo = new InMemoryProfesionalRepository();
        var usuarios = new FakeUsuarioRepository(existente);
        var sut = Crear(repo, usuarios);

        await sut.Invoking(s => s.CrearAsync(RequestValido(), Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task BajaAsync_ConTurnosActivos_LanzaConflict()
    {
        var repo = new InMemoryProfesionalRepository(TestData.ProfesionalActivo(10));
        repo.ConTurnosActivos.Add(10);
        var sut = Crear(repo);

        await sut.Invoking(s => s.BajaAsync(10, Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task BajaAsync_SinTurnos_MarcaDeletedAt()
    {
        var profesional = TestData.ProfesionalActivo(10);
        var sut = Crear(new InMemoryProfesionalRepository(profesional));

        await sut.BajaAsync(10, Ct);

        profesional.Usuario.DeletedAt.Should().Be(_clock.UtcNow);
    }
}
