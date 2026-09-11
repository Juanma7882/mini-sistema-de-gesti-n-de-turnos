namespace Turnos.Domain.Common;

/// <summary>
/// Raíz de las excepciones de negocio. La capa Application lanza subtipos
/// concretos (definidos en <c>Turnos.Application.Common.Exceptions</c>) y la Api
/// los traduce a <c>ProblemDetails</c> con el status code correspondiente.
/// No se lanza directamente.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
