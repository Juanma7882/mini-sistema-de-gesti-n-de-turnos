using Turnos.Domain.Usuarios;

namespace Turnos.Application.Abstractions;

/// <summary>Identidad del usuario autenticado, leída de los claims del JWT por la
/// Api. Los servicios la usan para forzar el alcance del rol Profesional (solo
/// ve sus turnos). El cliente nunca envía su <see cref="ProfesionalId"/>.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    int UsuarioId { get; }

    Rol Rol { get; }

    /// <summary>Id del profesional cuando <see cref="Rol"/> es
    /// <see cref="Rol.Profesional"/>; <c>null</c> para Admin.</summary>
    int? ProfesionalId { get; }
}
