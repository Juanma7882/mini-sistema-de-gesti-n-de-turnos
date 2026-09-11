using FluentValidation;

namespace Turnos.Application.Turnos;

public sealed class CambiarEstadoRequestValidator : AbstractValidator<CambiarEstadoRequest>
{
    public CambiarEstadoRequestValidator()
    {
        RuleFor(x => x.Estado)
            .IsInEnum()
            .WithMessage("Estado de turno desconocido.");
    }
}
