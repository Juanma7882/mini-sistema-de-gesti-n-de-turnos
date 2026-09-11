using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Turnos.Infrastructure.Persistence;

/// <summary>Solo para <c>dotnet ef</c> (migraciones). En runtime el contexto se
/// arma desde DI con la connection string de configuración; acá se usa una fija
/// para que las herramientas no necesiten arrancar el host de la Api.</summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=turnos.design.db")
            .Options;

        return new AppDbContext(options);
    }
}
