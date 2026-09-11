namespace Turnos.Infrastructure.Time;

/// <summary>Bindeada de la sección <c>Clock</c> (env var <c>Clock__TimeZoneId</c>).
/// Zona horaria de la clínica para calcular <c>IClock.LocalNow</c> (la hora
/// local naïve contra la que se valida <c>Turno.Inicio</c>).</summary>
public sealed class ClockOptions
{
    public const string SectionName = "Clock";

    /// <summary>Id IANA. Default: hora de Argentina.</summary>
    public string TimeZoneId { get; init; } = "America/Argentina/Buenos_Aires";
}
