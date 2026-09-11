using Microsoft.AspNetCore.SignalR;
using Turnos.Application.Turnos;

namespace Turnos.Api.Realtime;

/// <summary>Implementación de <see cref="ITurnoNotifier"/> sobre <see cref="TurnoHub"/>.
/// Vive en Api (no en Infrastructure) porque necesita <see cref="IHubContext{THub}"/>,
/// que depende del tipo del Hub, un concepto propio de ASP.NET Core.</summary>
internal sealed class SignalRTurnoNotifier(IHubContext<TurnoHub> hub) : ITurnoNotifier
{
    private const string Evento = "turnoCambiado";

    public async Task NotificarCambioAsync(TurnoDto turno, CancellationToken ct)
    {
        await hub.Clients.Group(TurnoNotificationGroups.Admins).SendAsync(Evento, turno, ct);
        await hub.Clients
            .Group(TurnoNotificationGroups.ParaProfesional(turno.Profesional.Id))
            .SendAsync(Evento, turno, ct);
    }
}
