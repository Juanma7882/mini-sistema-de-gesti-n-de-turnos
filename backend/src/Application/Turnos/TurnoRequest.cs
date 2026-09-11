namespace Turnos.Application.Turnos;

/// <summary>Body de <c>POST</c>/<c>PUT /turnos</c>. Son datos del turno, nunca el
/// estado: el estado se cambia solo por <c>PATCH /turnos/{id}/estado</c>.</summary>
public sealed record TurnoRequest
{
    public int PacienteId { get; init; }

    public int ProfesionalId { get; init; }

    /// <summary>Hora local naïve, precisión de minuto y a futuro
    /// (lo valida <c>TurnoRequestValidator</c>).</summary>
    public DateTime Inicio { get; init; }

    public string? Notas { get; init; }
}
