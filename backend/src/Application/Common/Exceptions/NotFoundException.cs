using Turnos.Domain.Common;

namespace Turnos.Application.Common.Exceptions;

/// <summary>Recurso inexistente. La Api lo mapea a <c>404</c>. También se usa
/// cuando un Profesional pide un turno ajeno (no se revela que existe).</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException Para(string recurso, object id) =>
        new($"{recurso} {id} no encontrado.");
}
