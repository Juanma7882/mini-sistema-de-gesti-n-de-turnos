namespace Turnos.Application.Pacientes;

/// <summary>Body de <c>POST</c>/<c>PUT /pacientes</c>. El servicio normaliza
/// <see cref="Nombre"/> y <see cref="Apellido"/> antes de persistir
/// (ver <c>TextoNormalizer.NombrePropio</c>).</summary>
public sealed record PacienteRequest
{
    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string Telefono { get; init; } = string.Empty;

    public string ObraSocial { get; init; } = string.Empty;
}
