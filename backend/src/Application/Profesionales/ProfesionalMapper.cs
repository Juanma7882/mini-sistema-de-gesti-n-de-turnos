using Turnos.Domain.Profesionales;

namespace Turnos.Application.Profesionales;

/// <summary>Mapeo entidad → DTO. Sin AutoMapper: métodos estáticos explícitos.</summary>
public static class ProfesionalMapper
{
    public static ProfesionalDto ToDto(Profesional profesional) => new()
    {
        Id = profesional.Id,
        Nombre = profesional.Usuario.Nombre,
        Apellido = profesional.Usuario.Apellido,
        Especialidad = profesional.Especialidad,
        CreatedAt = profesional.CreatedAt,
    };

    public static ProfesionalResumenDto ToResumen(Profesional profesional) => new()
    {
        Id = profesional.Id,
        Nombre = profesional.Usuario.Nombre,
        Apellido = profesional.Usuario.Apellido,
        Especialidad = profesional.Especialidad,
    };
}
