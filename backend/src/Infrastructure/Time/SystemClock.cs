using Microsoft.Extensions.Options;
using Turnos.Application.Abstractions;

namespace Turnos.Infrastructure.Time;

/// <summary>Reloj real. <see cref="UtcNow"/> para auditoría; <see cref="LocalNow"/>
/// es la hora de la clínica como <see cref="DateTimeKind.Unspecified"/>, para
/// comparar contra <c>Turno.Inicio</c>.</summary>
internal sealed class SystemClock : IClock
{
    private readonly TimeZoneInfo _timeZone;

    public SystemClock(IOptions<ClockOptions> options)
    {
        _timeZone = ResolverZona(options.Value.TimeZoneId);
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.SpecifyKind(
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone),
        DateTimeKind.Unspecified);

    private static TimeZoneInfo ResolverZona(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }
}
