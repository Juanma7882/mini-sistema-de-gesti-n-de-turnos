using Turnos.Domain.Turnos;

namespace Turnos.Application.Turnos;

/// <summary>Body de <c>PATCH /turnos/{id}/estado</c>.</summary>
public sealed record CambiarEstadoRequest
{
    public EstadoTurno Estado { get; init; }
}
