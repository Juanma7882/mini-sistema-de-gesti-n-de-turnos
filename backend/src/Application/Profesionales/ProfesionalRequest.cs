namespace Turnos.Application.Profesionales;

/// <summary>Body de <c>POST</c>/<c>PUT /profesionales</c>. El servicio normaliza
/// <see cref="Nombre"/> y <see cref="Apellido"/> antes de persistir.</summary>
public sealed record ProfesionalRequest
{
    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string Especialidad { get; init; } = string.Empty;
}
