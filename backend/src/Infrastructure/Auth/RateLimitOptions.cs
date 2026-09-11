namespace Turnos.Infrastructure.Auth;

/// <summary>Bindeada de la sección <c>RateLimit</c> (env vars <c>RateLimit__*</c>).
/// La usa <c>Api/DependencyInjection.AddLoginRateLimiting</c> para limitar
/// intentos de fuerza bruta contra <c>POST /auth/login</c>.</summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int PermitLimit { get; init; } = 5;

    public int WindowSeconds { get; init; } = 60;
}
