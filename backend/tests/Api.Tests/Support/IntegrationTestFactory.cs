using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Turnos.Api.Tests.Support;

/// <summary>Host real de la Api (<c>Program.cs</c> tal cual) contra un SQLite
/// temporal, migrado y sembrado al bootear (§6.1 ya llama
/// <c>MigrateAndSeedAsync()</c> ahí, no hace falta invocarlo acá).
///
/// Las variables de entorno se setean en el <b>constructor</b>, antes de que
/// cualquier test toque <c>Services</c>/<c>CreateClient()</c> (recién ahí
/// arranca <c>Program.cs</c> de verdad). Es necesario porque <c>AddApi</c> y
/// <c>AddInfrastructure</c> leen <c>IConfiguration</c> de forma <em>eager</em> al
/// registrar servicios (<c>configuration["Jwt:Secret"]</c>,
/// <c>GetConnectionString("Default")</c> como variables locales, no vía
/// <c>IOptions&lt;T&gt;</c>) — <c>ConfigureWebHost(b =&gt; b.ConfigureAppConfiguration(...))</c>
/// de <see cref="WebApplicationFactory{TEntryPoint}"/> llega <em>después</em> de
/// que esas líneas ya corrieron, así que no sirve para pisarlas. Las env vars sí
/// llegan a tiempo: <c>WebApplicationBuilder.CreateBuilder(args)</c> las lee como
/// fuente de configuración desde la primera línea de <c>Program.cs</c>.</summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"turnos-api-tests-{Guid.NewGuid():N}.db");

    // xUnit instancia esta clase como ICollectionFixture por reflection: tiene
    // que quedar con constructor parameterless (no acepta overloads). El
    // límite bajo de RateLimitTests se pisa aparte, ver WithLoginRateLimit.
    public IntegrationTestFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", $"Data Source={_dbPath}");
        Environment.SetEnvironmentVariable(
            "Jwt__Secret", "integration-tests-secret-not-for-production-32chars-min");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "turnos-api-tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "turnos-web-tests");
        Environment.SetEnvironmentVariable("Jwt__AccessMinutes", "15");
        Environment.SetEnvironmentVariable("Jwt__RefreshDays", "7");
        Environment.SetEnvironmentVariable("Seed__AdminPassword", LoginHelper.AdminPassword);
        Environment.SetEnvironmentVariable("Seed__ProfessionalPassword", LoginHelper.ProfesionalPassword);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins", "http://localhost:5173");

        // Alto por default a propósito: esta factory es compartida por toda la
        // colección "Api" (§10 preamble) y casi todas las clases usan
        // LoginHelper para su setup — con el default de producción (5/60s) el
        // rate limiter de /auth/login rompería tests que no tienen nada que
        // ver con rate limiting. Ver WithLoginRateLimit para el caso que sí
        // necesita probar el 429 de verdad.
        Environment.SetEnvironmentVariable("RateLimit__PermitLimit", "100000");
        Environment.SetEnvironmentVariable("RateLimit__WindowSeconds", "60");
    }

    /// <summary>Crea una instancia propia (no la compartida de <see cref="ApiCollection"/>)
    /// con un <c>PermitLimit</c> bajo para <see cref="RateLimitTests"/>. Pisa la
    /// env var recién acá, después del constructor de arriba y antes de que el
    /// caller dispare el primer <c>CreateClient()</c> — el host todavía no se
    /// construyó (es lazy), así que a tiempo. El caller es responsable de
    /// restaurar el valor alto en un <c>finally</c> al terminar, para no
    /// filtrarle este límite bajo a la factory compartida si su host todavía no
    /// se había construido.</summary>
    public static IntegrationTestFactory WithLoginRateLimit(int permitLimit)
    {
        var factory = new IntegrationTestFactory();
        Environment.SetEnvironmentVariable("RateLimit__PermitLimit", permitLimit.ToString());

        return factory;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch (IOException)
        {
            // el runner limpia el temp dir igual.
        }
    }
}
