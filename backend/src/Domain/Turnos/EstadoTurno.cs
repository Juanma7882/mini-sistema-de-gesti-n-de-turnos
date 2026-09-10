namespace Turnos.Domain.Turnos;

/// <summary>
/// Estado del turno. Se persiste como <c>int</c>; viaja como string en el JSON.
/// El valor <c>2</c> (<see cref="Cancelado"/>) está cableado en el filtro del
/// índice único parcial <c>(ProfesionalId, Inicio) WHERE Estado &lt;&gt; 2</c>:
/// si se reordena el enum, actualizar ese filtro.
/// <see cref="Cancelado"/> y <see cref="Atendido"/> son estados terminales.
/// </summary>
public enum EstadoTurno
{
    Pendiente = 0,
    Confirmado = 1,
    Cancelado = 2,
    Atendido = 3,
}
