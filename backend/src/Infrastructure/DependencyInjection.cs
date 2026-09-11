using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Turnos.Application.Abstractions;
using Turnos.Infrastructure.Auth;
using Turnos.Infrastructure.Persistence;
using Turnos.Infrastructure.Persistence.Repositories;
using Turnos.Infrastructure.Persistence.Seed;
using Turnos.Infrastructure.Time;

namespace Turnos.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra el <c>DbContext</c>, los repositorios y las
    /// implementaciones de las interfaces de <c>Application/Abstractions</c>.
    /// El seeder queda disponible para que la Api lo corra al arrancar.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=turnos.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<ClockOptions>(configuration.GetSection(ClockOptions.SectionName));

        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<DbSeeder>();

        return services;
    }

    /// <summary>Aplica migraciones pendientes y siembra datos de demo si la base
    /// está vacía. La Api la llama una vez al arrancar. Expone el trabajo sin
    /// filtrar <c>AppDbContext</c> ni <c>DbSeeder</c> (internos a Infrastructure).</summary>
    public static async Task MigrateAndSeedAsync(
        this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync(ct);
    }
}
