using FluentValidation;

namespace Turnos.Application.Profesionales;

public sealed class ProfesionalRequestValidator : AbstractValidator<ProfesionalRequest>
{
    public ProfesionalRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Apellido)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Especialidad)
            .NotEmpty()
            .MaximumLength(80);
    }
}
