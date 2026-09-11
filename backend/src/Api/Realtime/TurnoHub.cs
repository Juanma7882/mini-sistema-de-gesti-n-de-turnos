using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Turnos.Api.Auth;

namespace Turnos.Api.Realtime;

/// <summary>Hub de notificaciones de turnos. Cada conexión se suma a un grupo
/// según su rol, para que <see cref="SignalRTurnoNotifier"/> pueda avisar solo
/// a quien le corresponde: todos los Admin, o el Profesional dueño del turno.</summary>
[Authorize]
public sealed class TurnoHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user?.IsInRole(Roles.Admin) == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TurnoNotificationGroups.Admins);
        }

        var profesionalId = user?.FindFirst("profesionalId")?.Value;
        if (profesionalId is not null)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, TurnoNotificationGroups.ParaProfesional(profesionalId));
        }

        await base.OnConnectedAsync();
    }
}

internal static class TurnoNotificationGroups
{
    public const string Admins = "admins";

    public static string ParaProfesional(object profesionalId) => $"profesional-{profesionalId}";
}
