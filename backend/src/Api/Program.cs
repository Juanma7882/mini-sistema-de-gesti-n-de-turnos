using Turnos.Api;
using Turnos.Application;
using Turnos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Composición de capas: cada Add* registra solo lo suyo (ver DependencyInjection
// de cada proyecto). No se re-registra acá nada que ya viva en Application o
// Infrastructure (DbContext, validators, repos, JWT/refresh/hash services).
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

// Aplica migraciones pendientes y siembra datos de demo si la base está vacía.
// Idempotente: DbSeeder solo siembra si no hay usuarios.
await app.Services.MigrateAndSeedAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(Turnos.Api.DependencyInjection.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;
