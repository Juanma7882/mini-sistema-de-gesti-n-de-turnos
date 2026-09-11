namespace Turnos.Application.Turnos;

/// <summary>Avisa a los clientes conectados que un turno cambió (creado,
/// editado o cambio de estado), para que refresquen sus listados sin
/// recargar la página.</summary>
public interface ITurnoNotifier
{
    Task NotificarCambioAsync(TurnoDto turno, CancellationToken ct);
}
