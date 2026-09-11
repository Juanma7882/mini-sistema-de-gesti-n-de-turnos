namespace Turnos.Application.Profesionales;

/// <summary>Resumen de profesional embebido en <c>TurnoDto</c> (sin
/// <c>createdAt</c>).</summary>
public sealed record ProfesionalResumenDto
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public required string Apellido { get; init; }

    public required string Especialidad { get; init; }
}
