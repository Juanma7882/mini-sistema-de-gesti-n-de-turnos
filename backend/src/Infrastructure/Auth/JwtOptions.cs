namespace Turnos.Infrastructure.Auth;

/// <summary>Bindeada de la sección <c>Jwt</c> (env vars <c>Jwt__*</c>). La usa
/// <see cref="JwtTokenService"/> para firmar y la Api para validar.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = "turnos-api";

    public string Audience { get; init; } = "turnos-web";

    public int AccessMinutes { get; init; } = 15;

    public int RefreshDays { get; init; } = 7;
}
