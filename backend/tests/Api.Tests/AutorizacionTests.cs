using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AutorizacionTests(IntegrationTestFactory factory)
{
    [Theory]
    [InlineData("POST", "/api/pacientes")]
    [InlineData("PUT", "/api/pacientes/1")]
    [InlineData("DELETE", "/api/pacientes/1")]
    [InlineData("POST", "/api/profesionales")]
    [InlineData("PUT", "/api/profesionales/1")]
    [InlineData("DELETE", "/api/profesionales/1")]
    [InlineData("POST", "/api/turnos")]
    [InlineData("PUT", "/api/turnos/1")]
    public async Task Profesional_ContraEndpointSoloAdmin_Devuelve403(string metodo, string ruta)
    {
        using var profesional = await factory.AsProfesionalAsync();

        var http = new HttpMethod(metodo);
        var request = new HttpRequestMessage(http, ruta)
        {
            Content = http == HttpMethod.Post || http == HttpMethod.Put
                ? JsonContent.Create(new { })
                : null,
        };

        var response = await profesional.Http.SendAsync(request);

        // La autorización corre en el middleware, antes de llegar al binding/
        // validación del body -> 403 aunque el body esté vacío o sea inválido.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Profesional_AccedeTurnoAjeno_Devuelve404EnGetYEnPatchEstado()
    {
        using var admin = await factory.AsAdminAsync();
        using var profesional = await factory.AsProfesionalAsync();

        // Turno de OTRO profesional recién creado (no el de dra.gomez).
        var otroProfesional = await admin.Http.CrearProfesionalAsync();
        var paciente = await admin.Http.CrearPacienteAsync();
        var turnoAjeno = await admin.Http.CrearTurnoAsync(
            paciente.Id, otroProfesional.Id, ApiTestDataExtensions.SlotFuturoUnico());

        var get = await profesional.Http.GetAsync($"/api/turnos/{turnoAjeno.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var patch = await profesional.Http.PatchAsJsonAsync(
            $"/api/turnos/{turnoAjeno.Id}/estado",
            new CambiarEstadoRequest { Estado = EstadoTurno.Confirmado },
            ApiJson.Options);
        patch.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("GET", "/api/pacientes")]
    [InlineData("GET", "/api/profesionales")]
    [InlineData("GET", "/api/turnos")]
    [InlineData("GET", "/api/auth/me")]
    public async Task SinToken_EndpointProtegido_Devuelve401(string metodo, string ruta)
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), ruta));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
