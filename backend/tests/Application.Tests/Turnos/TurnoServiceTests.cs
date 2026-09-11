using FluentAssertions;
using Turnos.Application.Abstractions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Tests.Fakes;
using Turnos.Application.Turnos;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;

namespace Turnos.Application.Tests.Turnos;

public class TurnoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    // Posterior a FakeClock.LocalNow (2026-09-10), con precisión de minuto.
    private static readonly DateTime Futuro = new(2026, 10, 1, 15, 0, 0, DateTimeKind.Unspecified);

    private static TurnoRequest Req(
        int pacienteId = 1,
        int profesionalId = 10,
        DateTime? inicio = null,
        string? notas = null) => new()
    {
        PacienteId = pacienteId,
        ProfesionalId = profesionalId,
        Inicio = inicio ?? Futuro,
        Notas = notas,
    };

    // ---- Crear ----

    [Fact]
    public async Task CrearAsync_PacienteDadoDeBaja_LanzaNotFound()
    {
        var ctx = Construir(FakeCurrentUser.Admin(), pacientes: [TestData.PacienteDadoDeBaja(1)]);

        await ctx.Sut.Invoking(s => s.CrearAsync(Req(pacienteId: 1), Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CrearAsync_ProfesionalInexistente_LanzaNotFound()
    {
        var ctx = Construir(FakeCurrentUser.Admin());

        await ctx.Sut.Invoking(s => s.CrearAsync(Req(profesionalId: 999), Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CrearAsync_SlotOcupado_LanzaConflict()
    {
        var ocupado = TestData.Turno(1, 1, 10, Futuro);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: ocupado);

        await ctx.Sut.Invoking(s => s.CrearAsync(Req(inicio: Futuro), Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CrearAsync_Ok_DevuelvePendienteConPacienteYProfesionalEmbebidos()
    {
        var ctx = Construir(FakeCurrentUser.Admin());

        var dto = await ctx.Sut.CrearAsync(Req(notas: "  primera consulta  "), Ct);

        dto.Estado.Should().Be(EstadoTurno.Pendiente);
        dto.Inicio.Should().Be(Futuro);
        dto.Notas.Should().Be("primera consulta");
        dto.Paciente.Id.Should().Be(1);
        dto.Profesional.Id.Should().Be(10);
    }

    [Fact]
    public async Task CrearAsync_SlotLiberadoPorCancelacion_Ok()
    {
        var cancelado = TestData.Turno(1, 1, 10, Futuro, EstadoTurno.Cancelado);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: cancelado);

        var dto = await ctx.Sut.CrearAsync(Req(inicio: Futuro), Ct);

        dto.Id.Should().NotBe(1);
        dto.Estado.Should().Be(EstadoTurno.Pendiente);
    }

    [Fact]
    public async Task CrearAsync_CarreraEnElIndice_SePropagaComoConflict()
    {
        var ctx = Construir(FakeCurrentUser.Admin());
        ctx.Turnos.ForzarChoqueEnSave = true;

        await ctx.Sut.Invoking(s => s.CrearAsync(Req(), Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    // ---- Editar ----

    [Theory]
    [InlineData(EstadoTurno.Cancelado)]
    [InlineData(EstadoTurno.Atendido)]
    public async Task EditarAsync_TurnoEnEstadoTerminal_LanzaConflict(EstadoTurno terminal)
    {
        var turno = TestData.Turno(1, 1, 10, Futuro, terminal);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: turno);

        await ctx.Sut.Invoking(s => s.EditarAsync(1, Req(inicio: Futuro.AddHours(1)), Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task EditarAsync_MismoSlotDelPropioTurno_Ok()
    {
        var turno = TestData.Turno(1, 1, 10, Futuro);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: turno);

        var dto = await ctx.Sut.EditarAsync(1, Req(inicio: Futuro, notas: "reprogramada"), Ct);

        dto.Notas.Should().Be("reprogramada");
        turno.UpdatedAt.Should().Be(ctx.Clock.UtcNow);
    }

    [Fact]
    public async Task EditarAsync_Inexistente_LanzaNotFound()
    {
        var ctx = Construir(FakeCurrentUser.Admin());

        await ctx.Sut.Invoking(s => s.EditarAsync(42, Req(), Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ---- CambiarEstado ----

    [Fact]
    public async Task CambiarEstadoAsync_TransicionIlegal_LanzaConflict()
    {
        var turno = TestData.Turno(1, 1, 10, Futuro, EstadoTurno.Pendiente);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: turno);

        await ctx.Sut.Invoking(s => s.CambiarEstadoAsync(1, EstadoTurno.Atendido, Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CambiarEstadoAsync_PendienteAConfirmado_Ok()
    {
        var turno = TestData.Turno(1, 1, 10, Futuro, EstadoTurno.Pendiente);
        var ctx = Construir(FakeCurrentUser.Admin(), turnos: turno);

        var dto = await ctx.Sut.CambiarEstadoAsync(1, EstadoTurno.Confirmado, Ct);

        dto.Estado.Should().Be(EstadoTurno.Confirmado);
        turno.UpdatedAt.Should().Be(ctx.Clock.UtcNow);
    }

    [Fact]
    public async Task CambiarEstadoAsync_ProfesionalSobreTurnoAjeno_LanzaNotFound()
    {
        var ajeno = TestData.Turno(1, 1, 20, Futuro, EstadoTurno.Pendiente);
        var ctx = Construir(
            FakeCurrentUser.Profesional(profesionalId: 10),
            profesionales: [TestData.ProfesionalActivo(10), TestData.ProfesionalActivo(20)],
            turnos: ajeno);

        await ctx.Sut.Invoking(s => s.CambiarEstadoAsync(1, EstadoTurno.Confirmado, Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ---- Obtener / Listar (alcance por rol) ----

    [Fact]
    public async Task ObtenerAsync_ProfesionalSobreTurnoAjeno_LanzaNotFound()
    {
        var ajeno = TestData.Turno(1, 1, 20, Futuro);
        var ctx = Construir(
            FakeCurrentUser.Profesional(profesionalId: 10),
            profesionales: [TestData.ProfesionalActivo(10), TestData.ProfesionalActivo(20)],
            turnos: ajeno);

        await ctx.Sut.Invoking(s => s.ObtenerAsync(1, Ct))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ObtenerAsync_ProfesionalSobreTurnoPropio_Ok()
    {
        var propio = TestData.Turno(1, 1, 10, Futuro);
        var ctx = Construir(FakeCurrentUser.Profesional(profesionalId: 10), turnos: propio);

        var dto = await ctx.Sut.ObtenerAsync(1, Ct);

        dto.Id.Should().Be(1);
    }

    [Fact]
    public async Task ListarAsync_ComoProfesional_IgnoraProfesionalIdDelFiltro()
    {
        var ctx = ContextoConTurnosDeDosProfesionales(FakeCurrentUser.Profesional(profesionalId: 10));

        var pagina = await ctx.Sut.ListarAsync(
            new TurnoFiltro { ProfesionalId = 20 }, new PageRequest(), Ct);

        pagina.Items.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public async Task ListarAsync_ComoAdmin_RespetaProfesionalIdDelFiltro()
    {
        var ctx = ContextoConTurnosDeDosProfesionales(FakeCurrentUser.Admin());

        var pagina = await ctx.Sut.ListarAsync(
            new TurnoFiltro { ProfesionalId = 20 }, new PageRequest(), Ct);

        pagina.Items.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    // ---- helpers ----

    private static Contexto ContextoConTurnosDeDosProfesionales(ICurrentUser currentUser) =>
        Construir(
            currentUser,
            profesionales: [TestData.ProfesionalActivo(10), TestData.ProfesionalActivo(20)],
            turnos:
            [
                TestData.Turno(1, 1, 10, Futuro),
                TestData.Turno(2, 1, 20, Futuro.AddHours(1)),
            ]);

    private static Contexto Construir(
        ICurrentUser currentUser,
        IEnumerable<Paciente>? pacientes = null,
        IEnumerable<Profesional>? profesionales = null,
        params Turno[] turnos)
    {
        var pac = (pacientes ?? [TestData.PacienteActivo(1)]).ToList();
        var pro = (profesionales ?? [TestData.ProfesionalActivo(10)]).ToList();
        var clock = new FakeClock();
        var turnoRepo = new InMemoryTurnoRepository(pac, pro, turnos);

        var sut = new TurnoService(
            turnoRepo,
            new InMemoryPacienteRepository(pac.ToArray()),
            new InMemoryProfesionalRepository(pro.ToArray()),
            currentUser,
            clock);

        return new Contexto { Sut = sut, Turnos = turnoRepo, Clock = clock };
    }

    private sealed class Contexto
    {
        public required TurnoService Sut { get; init; }

        public required InMemoryTurnoRepository Turnos { get; init; }

        public required FakeClock Clock { get; init; }
    }
}
