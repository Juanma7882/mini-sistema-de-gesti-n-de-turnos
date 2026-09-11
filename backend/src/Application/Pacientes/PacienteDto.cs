namespace Turnos.Application.Pacientes;

/// <summary>Representación completa de un paciente en las respuestas.</summary>
public sealed record PacienteDto
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public required string Apellido { get; init; }

    public required string Telefono { get; init; }

    public required string ObraSocial { get; init; }

    public required DateTime CreatedAt { get; init; }
}
