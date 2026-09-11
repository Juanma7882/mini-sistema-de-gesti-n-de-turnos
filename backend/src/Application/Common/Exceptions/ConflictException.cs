using Turnos.Domain.Common;

namespace Turnos.Application.Common.Exceptions;

/// <summary>Choque con el estado actual: slot ocupado, transición de estado
/// ilegal, baja de un paciente/profesional con turnos activos, edición de un
/// turno ya cerrado. La Api lo mapea a <c>409</c>.</summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}
