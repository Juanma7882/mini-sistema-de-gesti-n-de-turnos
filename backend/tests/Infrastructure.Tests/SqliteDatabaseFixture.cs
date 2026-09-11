using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Turnos.Infrastructure;

namespace Turnos.Infrastructure.Tests;

/// <summary>Levanta un contenedor de DI real (<c>AddInfrastructure</c>) sobre un
/// archivo SQLite temporal, con el esquema migrado y los datos de seed. Se
/// descarta al terminar cada clase de test.</summary>
public sealed class SqliteDatabaseFixture : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"turnos-tests-{Guid.NewGuid():N}.db");

    private ServiceProvider _provider = null!;

    public IServiceScope CreateScope() => _provider.CreateScope();

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_dbPath}",
                ["Seed:AdminPassword"] = "Admin-test-123",
                ["Seed:ProfessionalPassword"] = "Profesional-test-123",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        _provider = services.BuildServiceProvider();

        await _provider.MigrateAndSeedAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();

        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch (IOException)
        {
            // el runner lo limpia con el temp dir
        }
    }
}
