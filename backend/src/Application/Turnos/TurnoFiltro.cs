using Turnos.Domain.Turnos;

namespace Turnos.Application.Turnos;

/// <summary>Filtros de <c>GET /turnos</c>. Todos opcionales. Para el rol
/// Profesional el servicio sobrescribe <see cref="ProfesionalId"/> con el suyo
/// antes de pasar el filtro al repositorio.</summary>
public sealed record TurnoFiltro
{
    /// <summary>Límite inferior (inclusive) de <c>Turno.Inicio</c>.</summary>
    public DateTime? Desde { get; init; }

    /// <summary>Límite superior (inclusive) de <c>Turno.Inicio</c>.</summary>
    public DateTime? Hasta { get; init; }

    public EstadoTurno? Estado { get; init; }

    public int? PacienteId { get; init; }

    public int? ProfesionalId { get; init; }
}
