using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Domain.Profesionales;

/// <summary>
/// Profesional que atiende turnos. Perfil de negocio; el nombre de la persona
/// vive en <see cref="Usuario"/> (única fuente de verdad), no acá. Baja
/// lógica vía <see cref="Usuario.DeletedAt"/> del usuario asociado (query
/// filter global en Infrastructure); nunca se hace hard-delete.
/// </summary>
public class Profesional
{
    public int Id { get; set; }

    public string Especialidad { get; set; } = string.Empty;

    /// <summary>UTC, seteado en la capa Application vía <c>IClock.UtcNow</c>.</summary>
    public DateTime CreatedAt { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();

    /// <summary>Cuenta de acceso asociada. Obligatoria: un profesional nunca
    /// existe sin usuario (FK 1:1 requerida, vive de este lado).</summary>
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
