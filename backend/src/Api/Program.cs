var builder = WebApplication.CreateBuilder(args);

// Composicion de capas (AddApplication / AddInfrastructure / AddApi) se agrega al implementar.

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;
