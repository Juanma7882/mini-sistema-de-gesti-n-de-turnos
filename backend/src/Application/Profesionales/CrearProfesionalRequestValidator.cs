using FluentValidation;

namespace Turnos.Application.Profesionales;

public sealed class CrearProfesionalRequestValidator : AbstractValidator<CrearProfesionalRequest>
{
    public CrearProfesionalRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Apellido)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Especialidad)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}
