using Turnos.Application.Turnos;

namespace Turnos.Application.Tests.Fakes;

internal sealed class RecordingTurnoNotifier : ITurnoNotifier
{
    public List<TurnoDto> Notificados { get; } = [];

    public Task NotificarCambioAsync(TurnoDto turno, CancellationToken ct)
    {
        Notificados.Add(turno);
        return Task.CompletedTask;
    }
}
