using FluentAssertions;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Profesionales;
using Turnos.Application.Tests.Fakes;

namespace Turnos.Application.Tests.Profesionales;

public class ProfesionalServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeClock _clock = new();

    private ProfesionalService Crear(InMemoryProfesionalRepository repository) => new(repository, _clock);

    [Fact]
    public async Task CrearAsync_NormalizaNombreYApellido()
    {
        var repo = new InMemoryProfesionalRepository();
        var sut = Crear(repo);

        var dto = await sut.CrearAsync(
            new ProfesionalRequest { Nombre = "laURA", Apellido = "GÓMEZ", Especialidad = " Pediatría " },
            Ct);

        dto.Nombre.Should().Be("Laura");
        dto.Apellido.Should().Be("Gómez");
        dto.Especialidad.Should().Be("Pediatría");
        dto.CreatedAt.Should().Be(_clock.UtcNow);
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

        profesional.DeletedAt.Should().Be(_clock.UtcNow);
    }
}
