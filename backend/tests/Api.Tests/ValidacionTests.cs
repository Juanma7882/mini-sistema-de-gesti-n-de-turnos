using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Pacientes;
using Turnos.Application.Profesionales;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class ValidacionTests(IntegrationTestFactory factory)
{
    [Fact]
    public async Task CrearTurno_InicioEnElPasado_Devuelve400ConErrorDeInicio()
    {
        using var admin = await factory.AsAdminAsync();
        var paciente = await admin.Http.CrearPacienteAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/turnos", new TurnoRequest
        {
            PacienteId = paciente.Id,
            ProfesionalId = profesional.Id,
            Inicio = new DateTime(2020, 1, 1, 10, 0, 0),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errores = await LeerErroresAsync(response);
        errores.Should().ContainKey("Inicio");
    }

    [Fact]
    public async Task CrearPaciente_CamposVacios_Devuelve400ConErroresPorCampo()
    {
        using var admin = await factory.AsAdminAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/pacientes", new PacienteRequest());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errores = await LeerErroresAsync(response);
        errores.Keys.Should().Contain(["Nombre", "Apellido", "Telefono", "ObraSocial"]);
    }

    [Fact]
    public async Task CrearProfesional_CamposVacios_Devuelve400ConErroresPorCampo()
    {
        using var admin = await factory.AsAdminAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/profesionales", new ProfesionalRequest());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errores = await LeerErroresAsync(response);
        errores.Keys.Should().Contain(["Nombre", "Apellido", "Especialidad"]);
    }

    /// <summary>Regresión del bug encontrado al planificar este test: con
    /// <c>SuppressModelStateInvalidFilter</c> (§6), un valor de enum que no
    /// matchea ningún nombre dejaba <c>request</c> en <c>null</c> y el action
    /// reventaba con <c>NullReferenceException</c> → 500. <c>ValidationFilter</c>
    /// ahora también revisa <c>ModelState</c>, no solo FluentValidation.</summary>
    [Fact]
    public async Task CambiarEstado_ValorFueraDelEnum_Devuelve400YNoModificaElTurno()
    {
        using var admin = await factory.AsAdminAsync();
        var paciente = await admin.Http.CrearPacienteAsync();
        var profesional = await admin.Http.CrearProfesionalAsync();
        var turno = await admin.Http.CrearTurnoAsync(
            paciente.Id, profesional.Id, ApiTestDataExtensions.SlotFuturoUnico());

        var response = await admin.Http.PatchAsync(
            $"/api/turnos/{turno.Id}/estado", JsonContent.Create(new { estado = "NoExiste" }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var actual = await admin.Http.GetFromJsonAsync<TurnoDto>($"/api/turnos/{turno.Id}", ApiJson.Options);
        actual!.Estado.Should().Be(EstadoTurno.Pendiente);
    }

    private static async Task<IReadOnlyDictionary<string, string[]>> LeerErroresAsync(HttpResponseMessage response)
    {
        var problema = await response.Content.ReadFromJsonAsync<ProblemaDeValidacion>();

        return problema!.Errors;
    }

    private sealed record ProblemaDeValidacion(IReadOnlyDictionary<string, string[]> Errors);
}
