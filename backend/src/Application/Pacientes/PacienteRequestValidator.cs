using FluentValidation;

namespace Turnos.Application.Pacientes;

public sealed class PacienteRequestValidator : AbstractValidator<PacienteRequest>
{
    public PacienteRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Apellido)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Telefono)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.ObraSocial)
            .NotEmpty()
            .MaximumLength(80);
    }
}
