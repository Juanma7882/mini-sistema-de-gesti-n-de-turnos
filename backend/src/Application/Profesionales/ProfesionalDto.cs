namespace Turnos.Application.Profesionales;

/// <summary>Representación completa de un profesional en las respuestas.</summary>
public sealed record ProfesionalDto
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public required string Apellido { get; init; }

    public required string Especialidad { get; init; }

    public required DateTime CreatedAt { get; init; }
}
