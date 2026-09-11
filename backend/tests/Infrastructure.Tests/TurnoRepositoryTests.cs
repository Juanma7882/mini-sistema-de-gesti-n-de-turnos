using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;
using Turnos.Application.Turnos;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Infrastructure.Tests;

/// <summary>Verifica contra SQLite real las garantías que los fakes de
/// Application no pueden dar: los dos índices únicos parciales de <c>Turnos</c>
/// y su traducción a <see cref="ConflictException"/>.</summary>
public sealed class TurnoRepositoryTests(SqliteDatabaseFixture fixture)
    : IClassFixture<SqliteDatabaseFixture>
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTime Inicio = new(2027, 3, 1, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task SaveChanges_MismoProfesionalMismoInicio_NoCancelado_LanzaConflict()
    {
        using var scope = fixture.CreateScope();
        var (paciente1, _, profesional) = await SembrarParAsync(scope, sufijo: "slot");
        var paciente2 = await NuevoPacienteAsync(scope, "slot-2");
        var turnos = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();

        await turnos.AddAsync(NuevoTurno(paciente1.Id, profesional.Id, Inicio, EstadoTurno.Pendiente), Ct);
        await turnos.SaveChangesAsync(Ct);

        await turnos.AddAsync(NuevoTurno(paciente2.Id, profesional.Id, Inicio, EstadoTurno.Pendiente), Ct);

        await FluentActions.Invoking(() => turnos.SaveChangesAsync(Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SaveChanges_MismoInicioPeroPrimeroCancelado_Ok()
    {
        using var scope = fixture.CreateScope();
        var (paciente1, _, profesional) = await SembrarParAsync(scope, sufijo: "slot-libre");
        var paciente2 = await NuevoPacienteAsync(scope, "slot-libre-2");
        var turnos = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();

        await turnos.AddAsync(NuevoTurno(paciente1.Id, profesional.Id, Inicio, EstadoTurno.Cancelado), Ct);
        await turnos.SaveChangesAsync(Ct);

        await turnos.AddAsync(NuevoTurno(paciente2.Id, profesional.Id, Inicio, EstadoTurno.Pendiente), Ct);

        await FluentActions.Invoking(() => turnos.SaveChangesAsync(Ct))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveChanges_ParProfesionalPacienteConDosTurnosActivos_LanzaConflict()
    {
        using var scope = fixture.CreateScope();
        var (paciente, _, profesional) = await SembrarParAsync(scope, sufijo: "par");
        var turnos = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();

        await turnos.AddAsync(
            NuevoTurno(paciente.Id, profesional.Id, Inicio, EstadoTurno.Pendiente), Ct);
        await turnos.SaveChangesAsync(Ct);

        // Otro horario, mismo (profesional, paciente), ambos activos.
        await turnos.AddAsync(
            NuevoTurno(paciente.Id, profesional.Id, Inicio.AddHours(3), EstadoTurno.Confirmado), Ct);

        await FluentActions.Invoking(() => turnos.SaveChangesAsync(Ct))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SaveChanges_MismoParPeroUnoAtendido_Ok()
    {
        using var scope = fixture.CreateScope();
        var (paciente, _, profesional) = await SembrarParAsync(scope, sufijo: "par-cerrado");
        var turnos = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();

        await turnos.AddAsync(
            NuevoTurno(paciente.Id, profesional.Id, Inicio, EstadoTurno.Atendido), Ct);
        await turnos.SaveChangesAsync(Ct);

        await turnos.AddAsync(
            NuevoTurno(paciente.Id, profesional.Id, Inicio.AddHours(3), EstadoTurno.Pendiente), Ct);

        await FluentActions.Invoking(() => turnos.SaveChangesAsync(Ct))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPagedAsync_FiltraPorRangoYTraePacienteYProfesional()
    {
        using var scope = fixture.CreateScope();
        var (paciente, _, profesional) = await SembrarParAsync(scope, sufijo: "listado");
        var turnos = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();

        await turnos.AddAsync(NuevoTurno(paciente.Id, profesional.Id, Inicio, EstadoTurno.Pendiente), Ct);
        await turnos.SaveChangesAsync(Ct);

        var (items, total) = await turnos.GetPagedAsync(
            new TurnoFiltro
            {
                ProfesionalId = profesional.Id,
                Desde = Inicio.AddDays(-1),
                Hasta = Inicio.AddDays(1),
            },
            page: 1,
            pageSize: 20,
            Ct);

        total.Should().Be(1);
        var turno = items.Single();
        turno.Paciente.Should().NotBeNull();
        turno.Profesional.Id.Should().Be(profesional.Id);
    }

    private static async Task<(Paciente Paciente, Paciente _, Profesional Profesional)> SembrarParAsync(
        IServiceScope scope, string sufijo)
    {
        var paciente = await NuevoPacienteAsync(scope, sufijo);

        var profesionales = scope.ServiceProvider.GetRequiredService<IProfesionalRepository>();
        var profesional = new Profesional
        {
            Especialidad = "Test",
            CreatedAt = DateTime.UtcNow,
            Usuario = new Usuario
            {
                Nombre = "Pro",
                Apellido = sufijo,
                Email = $"pro.{sufijo}@test.local",
                PasswordHash = "hash:test",
                Rol = Rol.Profesional,
                CreatedAt = DateTime.UtcNow,
            },
        };
        await profesionales.AddAsync(profesional, Ct);
        await profesionales.SaveChangesAsync(Ct);

        return (paciente, paciente, profesional);
    }

    private static async Task<Paciente> NuevoPacienteAsync(IServiceScope scope, string sufijo)
    {
        var pacientes = scope.ServiceProvider.GetRequiredService<IPacienteRepository>();
        var paciente = new Paciente
        {
            Nombre = "Pac",
            Apellido = sufijo,
            Telefono = "000",
            ObraSocial = "Test",
            CreatedAt = DateTime.UtcNow,
        };
        await pacientes.AddAsync(paciente, Ct);
        await pacientes.SaveChangesAsync(Ct);
        return paciente;
    }

    private static Turno NuevoTurno(int pacienteId, int profesionalId, DateTime inicio, EstadoTurno estado) => new()
    {
        PacienteId = pacienteId,
        ProfesionalId = profesionalId,
        Inicio = inicio,
        Estado = estado,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };
}
