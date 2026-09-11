using Turnos.Domain.Pacientes;

namespace Turnos.Application.Pacientes;

/// <summary>Mapeo entidad → DTO. Sin AutoMapper: métodos estáticos explícitos.</summary>
public static class PacienteMapper
{
    public static PacienteDto ToDto(Paciente paciente) => new()
    {
        Id = paciente.Id,
        Nombre = paciente.Nombre,
        Apellido = paciente.Apellido,
        Telefono = paciente.Telefono,
        ObraSocial = paciente.ObraSocial,
        CreatedAt = paciente.CreatedAt,
    };

    public static PacienteResumenDto ToResumen(Paciente paciente) => new()
    {
        Id = paciente.Id,
        Nombre = paciente.Nombre,
        Apellido = paciente.Apellido,
        Telefono = paciente.Telefono,
        ObraSocial = paciente.ObraSocial,
    };
}
