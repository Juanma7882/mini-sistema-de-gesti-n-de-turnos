using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Domain.Profesionales;

/// <summary>
/// Profesional que atiende turnos. Baja lógica vía <see cref="DeletedAt"/>
/// (query filter global en Infrastructure); nunca se hace hard-delete.
/// </summary>
public class Profesional
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Especialidad { get; set; } = string.Empty;

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Marca de baja lógica. <c>null</c> ⇒ activo.</summary>
    public DateTime? DeletedAt { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();

    /// <summary>Cuenta de acceso asociada (1:0..1; la FK vive en <see cref="Usuario"/>).</summary>
    public Usuario? Usuario { get; set; }
}
