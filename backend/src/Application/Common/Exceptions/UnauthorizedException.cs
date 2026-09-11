using Turnos.Domain.Common;

namespace Turnos.Application.Common.Exceptions;

/// <summary>Credenciales o refresh token inválidos. La Api la mapea a <c>401</c>.
/// El mensaje es genérico a propósito (no distingue "email inexistente" de
/// "password incorrecta").</summary>
public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
