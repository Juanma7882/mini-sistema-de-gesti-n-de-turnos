namespace Turnos.Domain.Turnos;

/// <summary>
/// Máquina de estados del turno. Función pura y sin noción de rol: solo dice qué
/// flechas del diagrama son legales. Qué transición legal puede disparar cada rol
/// (Admin vs Profesional) y sobre qué turnos se valida en la capa Application.
/// <see cref="EstadoTurno.Cancelado"/> y <see cref="EstadoTurno.Atendido"/> son terminales.
/// </summary>
public static class MaquinaEstados
{
    private static readonly IReadOnlySet<EstadoTurno> Ninguna = new HashSet<EstadoTurno>();

    private static readonly IReadOnlySet<EstadoTurno> DesdePendiente =
        new HashSet<EstadoTurno> { EstadoTurno.Confirmado, EstadoTurno.Cancelado };

    private static readonly IReadOnlySet<EstadoTurno> DesdeConfirmado =
        new HashSet<EstadoTurno> { EstadoTurno.Atendido, EstadoTurno.Cancelado };

    /// <summary>
    /// Estados alcanzables desde <paramref name="desde"/> en un solo paso.
    /// Conjunto vacío si el estado es terminal. Nunca incluye a <paramref name="desde"/>.
    /// </summary>
    public static IReadOnlySet<EstadoTurno> TransicionesLegales(EstadoTurno desde) => desde switch
    {
        EstadoTurno.Pendiente => DesdePendiente,
        EstadoTurno.Confirmado => DesdeConfirmado,
        _ => Ninguna, // Cancelado y Atendido son terminales
    };

    /// <summary>
    /// <c>true</c> si se puede pasar de <paramref name="desde"/> a <paramref name="hacia"/>.
    /// Quedarse en el mismo estado (<c>desde == hacia</c>) no es una transición válida.
    /// </summary>
    public static bool EsTransicionValida(EstadoTurno desde, EstadoTurno hacia) =>
        TransicionesLegales(desde).Contains(hacia);
}
