namespace Turnos.Application.Pacientes;

/// <summary>Resumen de paciente embebido en <c>TurnoDto</c> (sin
/// <c>createdAt</c>), para que el listado de turnos no dispare más queries.</summary>
public sealed record PacienteResumenDto
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public required string Apellido { get; init; }

    public required string Telefono { get; init; }

    public required string ObraSocial { get; init; }
}
