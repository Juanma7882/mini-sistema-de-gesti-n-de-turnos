using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Api.Tests.Turnos;

[Collection(ApiCollection.Name)]
public sealed class AntiDobleTurnoTests(IntegrationTestFactory factory)
{
    [Fact]
    public async Task Post_MismoProfesionalMismoInicio_SegundoDevuelve409()
    {
        using var admin = await factory.AsAdminAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();
        var paciente1 = await admin.Http.CrearPacienteAsync();
        var paciente2 = await admin.Http.CrearPacienteAsync();
        var inicio = ApiTestDataExtensions.SlotFuturoUnico();

        var primero = await admin.Http.PostAsJsonAsync("/api/turnos", new TurnoRequest
        {
            PacienteId = paciente1.Id,
            ProfesionalId = profesional.Id,
            Inicio = inicio,
        });
        primero.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundo = await admin.Http.PostAsJsonAsync("/api/turnos", new TurnoRequest
        {
            PacienteId = paciente2.Id,
            ProfesionalId = profesional.Id,
            Inicio = inicio,
        });
        segundo.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_TrasCancelarElPrimero_LiberaElSlotParaOtroTurno()
    {
        using var admin = await factory.AsAdminAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();
        var paciente1 = await admin.Http.CrearPacienteAsync();
        var paciente2 = await admin.Http.CrearPacienteAsync();
        var inicio = ApiTestDataExtensions.SlotFuturoUnico();

        var turno = await admin.Http.CrearTurnoAsync(paciente1.Id, profesional.Id, inicio);

        var cancelar = await admin.Http.PatchAsJsonAsync(
            $"/api/turnos/{turno.Id}/estado",
            new CambiarEstadoRequest { Estado = EstadoTurno.Cancelado },
            ApiJson.Options);
        cancelar.StatusCode.Should().Be(HttpStatusCode.OK);

        var recrear = await admin.Http.PostAsJsonAsync("/api/turnos", new TurnoRequest
        {
            PacienteId = paciente2.Id,
            ProfesionalId = profesional.Id,
            Inicio = inicio,
        });
        recrear.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
