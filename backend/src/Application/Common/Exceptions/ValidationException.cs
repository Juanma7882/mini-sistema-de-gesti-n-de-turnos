using Turnos.Domain.Common;

namespace Turnos.Application.Common.Exceptions;

/// <summary>Falla de validación de entrada. La Api la mapea a <c>400</c> con el
/// diccionario <see cref="Errors"/> (campo → mensajes) dentro del
/// <c>ProblemDetails</c>. La dispara el filtro de validación de la Api al correr
/// el <c>IValidator&lt;T&gt;</c> de FluentValidation.</summary>
public sealed class ValidationException : DomainException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Una o más validaciones fallaron.") =>
        Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
