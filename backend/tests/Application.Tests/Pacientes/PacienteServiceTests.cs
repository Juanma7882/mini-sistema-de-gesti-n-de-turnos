using FluentAssertions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Pacientes;
using Turnos.Application.Tests.Fakes;
using Turnos.Domain.Pacientes;

namespace Turnos.Application.Tests.Pacientes;

public class PacienteServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeClock _clock = new();

    private PacienteService Crear(InMemoryPacienteRepository repository) => new(repository, _clock);

    [Fact]
    public async Task CrearAsync_NormalizaNombreYApellido_YSeteaCreatedAt()
    {
        var repo = new InMemoryPacienteRepository();
        var sut = Crear(repo);

        var dto = await sut.CrearAsync(
            new PacienteRequest
            {
                Nombre = "  jUAN  cARLOS ",
                Apellido = "góMEZ",
                Telefono = " 111 ",
                ObraSocial = " Swiss Medical ",
            },
            Ct);

        dto.Nombre.Should().Be("Juan Carlos");
        dto.Apellido.Should().Be("Gómez");
        dto.Telefono.Should().Be("111");
        dto.ObraSocial.Should().Be("Swiss Medical");
        dto.CreatedAt.Should().Be(_clock.UtcNow);
        repo.SaveChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNotFound()
    {
        var sut = Crear(new InMemoryPacienteRepository());

        await sut.Invoking(s => s.ObtenerAsync(99, Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EditarAsync_NormalizaYPersiste()
    {
        var paciente = TestData.PacienteActivo(1);
        var repo = new InMemoryPacienteRepository(paciente);
        var sut = Crear(repo);

        var dto = await sut.EditarAsync(
            1,
            new PacienteRequest
            {
                Nombre = "PEDRO",
                Apellido = "lópez",
                Telefono = "222",
                ObraSocial = "PAMI",
            },
            Ct);

        dto.Nombre.Should().Be("Pedro");
        dto.Apellido.Should().Be("López");
        paciente.Nombre.Should().Be("Pedro");
    }

    [Fact]
    public async Task BajaAsync_ConTurnosActivos_LanzaConflict()
    {
        var repo = new InMemoryPacienteRepository(TestData.PacienteActivo(1));
        repo.ConTurnosActivos.Add(1);
        var sut = Crear(repo);

        await sut.Invoking(s => s.BajaAsync(1, Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task BajaAsync_SinTurnos_MarcaDeletedAt()
    {
        var paciente = TestData.PacienteActivo(1);
        var sut = Crear(new InMemoryPacienteRepository(paciente));

        await sut.BajaAsync(1, Ct);

        paciente.DeletedAt.Should().Be(_clock.UtcNow);
    }

    [Fact]
    public async Task ListarAsync_FiltraPorNombreOApellido_SinDistinguirMayusculas()
    {
        var repo = new InMemoryPacienteRepository(
            TestData.PacienteActivo(1, "Ana", "Diaz"),
            TestData.PacienteActivo(2, "Bruno", "Perez"),
            TestData.PacienteActivo(3, "Carla", "Diaz"));
        var sut = Crear(repo);

        var pagina = await sut.ListarAsync("diaz", new PageRequest(), Ct);

        pagina.Total.Should().Be(2);
        pagina.Items.Select(p => p.Id).Should().BeEquivalentTo([1, 3]);
    }

    [Fact]
    public async Task ListarAsync_NoDevuelvePacientesDadosDeBaja()
    {
        var repo = new InMemoryPacienteRepository(
            TestData.PacienteActivo(1),
            TestData.PacienteDadoDeBaja(2));
        var sut = Crear(repo);

        var pagina = await sut.ListarAsync(null, new PageRequest(), Ct);

        pagina.Items.Should().ContainSingle().Which.Id.Should().Be(1);
    }
}
