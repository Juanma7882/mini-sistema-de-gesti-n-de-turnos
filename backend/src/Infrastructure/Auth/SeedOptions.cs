namespace Turnos.Infrastructure.Auth;

/// <summary>Bindeada de la sección <c>Seed</c> (env vars <c>Seed__*</c>).
/// Contraseñas de las cuentas de demo. Si vienen vacías, <see cref="DbSeeder"/>
/// usa un fallback fijo para desarrollo.</summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminPassword { get; init; } = string.Empty;

    public string ProfessionalPassword { get; init; } = string.Empty;
}
