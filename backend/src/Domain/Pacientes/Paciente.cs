using Turnos.Domain.Turnos;

namespace Turnos.Domain.Pacientes;

/// <summary>
/// Paciente de la clínica. Baja lógica vía <see cref="DeletedAt"/>
/// (query filter global en Infrastructure); nunca se hace hard-delete.
/// </summary>
public class Paciente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string ObraSocial { get; set; } = string.Empty;

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Marca de baja lógica. <c>null</c> ⇒ activo.</summary>
    public DateTime? DeletedAt { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
