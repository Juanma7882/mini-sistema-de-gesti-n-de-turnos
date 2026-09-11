using Turnos.Domain.Common;

namespace Turnos.Application.Common.Exceptions;

/// <summary>Autenticado pero sin permiso para la acción. La Api la mapea a
/// <c>403</c>. El grueso de la autorización por rol vive en atributos
/// <c>[Authorize]</c> de la Api; esta excepción cubre los casos que dependen de
/// datos (no solo del rol).</summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
