using Turnos.Application.Abstractions;

namespace Turnos.Application.Tests.Fakes;

/// <summary>Reloj fijo y ajustable para tests.</summary>
public sealed class FakeClock : IClock
{
    public FakeClock(DateTime? utcNow = null, DateTime? localNow = null)
    {
        UtcNow = utcNow ?? new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);
        LocalNow = localNow ?? new DateTime(2026, 9, 10, 9, 0, 0, DateTimeKind.Unspecified);
    }

    public DateTime UtcNow { get; set; }

    public DateTime LocalNow { get; set; }
}
