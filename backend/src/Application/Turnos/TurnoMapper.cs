using Turnos.Application.Pacientes;
using Turnos.Application.Profesionales;
using Turnos.Domain.Turnos;

namespace Turnos.Application.Turnos;

/// <summary>Mapeo entidad → DTO. Requiere <c>Paciente</c> y <c>Profesional</c>
/// cargados (el repositorio los trae con <c>Include</c>).</summary>
public static class TurnoMapper
{
    public static TurnoDto ToDto(Turno turno) => new()
    {
        Id = turno.Id,
        Inicio = turno.Inicio,
        Estado = turno.Estado,
        Notas = turno.Notas,
        Paciente = PacienteMapper.ToResumen(turno.Paciente),
        Profesional = ProfesionalMapper.ToResumen(turno.Profesional),
    };
}
