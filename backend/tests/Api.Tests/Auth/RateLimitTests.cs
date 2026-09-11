using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Auth;

namespace Turnos.Api.Tests.Auth;

/// <summary>Usa su <b>propia</b> <see cref="IntegrationTestFactory"/> con un
/// <c>PermitLimit</c> bajo (no la del <see cref="ApiCollection"/> compartido,
/// que queda con el default alto justamente para no romperse por estos
/// tests). Igual va en la colección "Api" para garantizar que no corre en
/// paralelo con el resto: sino, la variable de entorno de proceso que pisa
/// para bajar el límite podría filtrarse a la factory compartida si su host
/// todavía no se construyó (ver el <c>finally</c> de abajo).</summary>
[Collection(ApiCollection.Name)]
public sealed class RateLimitTests
{
    [Fact]
    public async Task Login_SuperaElLimitePorIp_Devuelve429()
    {
        using var factory = IntegrationTestFactory.WithLoginRateLimit(permitLimit: 3);
        try
        {
            using var client = factory.CreateClient();
            var credencialesInvalidas = new LoginRequest
            {
                Email = LoginHelper.AdminEmail,
                Password = "password-incorrecta",
            };

            for (var intento = 0; intento < 3; intento++)
            {
                var respuesta = await client.PostAsJsonAsync("/api/auth/login", credencialesInvalidas);
                respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            var rechazada = await client.PostAsJsonAsync("/api/auth/login", credencialesInvalidas);
            rechazada.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }
        finally
        {
            // Vuelve al default alto que usa la factory compartida del resto
            // de Api.Tests — ver IntegrationTestFactory().
            Environment.SetEnvironmentVariable("RateLimit__PermitLimit", "100000");
        }
    }
}
