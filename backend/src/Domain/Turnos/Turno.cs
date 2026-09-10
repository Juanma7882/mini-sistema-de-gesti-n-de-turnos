using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;

namespace Turnos.Domain.Turnos;

/// <summary>
/// Turno de un paciente con un profesional en un instante concreto.
/// No se borra: darlo de baja es pasarlo a <see cref="EstadoTurno.Cancelado"/>.
/// El anti doble-turno lo garantiza el índice único parcial
/// <c>(ProfesionalId, Inicio) WHERE Estado &lt;&gt; Cancelado</c> (ver Infrastructure).
/// </summary>
public class Turno
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public Paciente Paciente { get; set; } = null!;

    public int ProfesionalId { get; set; }

    public Profesional Profesional { get; set; } = null!;

    /// <summary>
    /// Hora local naïve (<see cref="DateTimeKind.Unspecified"/>), precisión de minuto.
    /// Serializa sin <c>Z</c> ni offset: <c>"2026-09-15T15:00:00"</c>.
    /// </summary>
    public DateTime Inicio { get; set; }

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    public string? Notas { get; set; }

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC. Se re-setea en cada modificación.</summary>
    public DateTime UpdatedAt { get; set; }
}
