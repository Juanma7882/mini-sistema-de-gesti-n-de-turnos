using Turnos.Application.Pacientes;
using Turnos.Application.Profesionales;
using Turnos.Domain.Turnos;

namespace Turnos.Application.Turnos;

/// <summary>Turno con los resúmenes de paciente y profesional embebidos, para
/// que el listado se pinte sin queries extra.</summary>
public sealed record TurnoDto
{
    public required int Id { get; init; }

    /// <summary>Hora local naïve, precisión de minuto. Serializa sin <c>Z</c>
    /// ni offset (<c>"2026-09-15T15:00:00"</c>).</summary>
    public required DateTime Inicio { get; init; }

    public required EstadoTurno Estado { get; init; }

    public string? Notas { get; init; }

    public required PacienteResumenDto Paciente { get; init; }

    public required ProfesionalResumenDto Profesional { get; init; }
}
