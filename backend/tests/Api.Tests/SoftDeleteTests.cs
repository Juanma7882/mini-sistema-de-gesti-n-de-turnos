using System.Net;
using FluentAssertions;
using Turnos.Api.Tests.Support;

namespace Turnos.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class SoftDeleteTests(IntegrationTestFactory factory)
{
    [Fact]
    public async Task BajaPaciente_ConTurnoActivo_Devuelve409()
    {
        using var admin = await factory.AsAdminAsync();
        var paciente = await admin.Http.CrearPacienteAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();
        await admin.Http.CrearTurnoAsync(paciente.Id, profesional.Id, ApiTestDataExtensions.SlotFuturoUnico());

        var response = await admin.Http.DeleteAsync($"/api/pacientes/{paciente.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BajaPaciente_SinTurnosActivos_Devuelve204YDesaparece()
    {
        using var admin = await factory.AsAdminAsync();
        var paciente = await admin.Http.CrearPacienteAsync();

        var baja = await admin.Http.DeleteAsync($"/api/pacientes/{paciente.Id}");
        baja.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await admin.Http.GetAsync($"/api/pacientes/{paciente.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BajaProfesional_ConTurnoActivo_Devuelve409()
    {
        using var admin = await factory.AsAdminAsync();
        var paciente = await admin.Http.CrearPacienteAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();
        await admin.Http.CrearTurnoAsync(paciente.Id, profesional.Id, ApiTestDataExtensions.SlotFuturoUnico());

        var response = await admin.Http.DeleteAsync($"/api/profesionales/{profesional.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BajaProfesional_SinTurnosActivos_Devuelve204YDesaparece()
    {
        using var admin = await factory.AsAdminAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();

        var baja = await admin.Http.DeleteAsync($"/api/profesionales/{profesional.Id}");
        baja.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await admin.Http.GetAsync($"/api/profesionales/{profesional.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
