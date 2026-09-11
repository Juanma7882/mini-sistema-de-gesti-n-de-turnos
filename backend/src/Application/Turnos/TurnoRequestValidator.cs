using FluentValidation;
using Turnos.Application.Abstractions;

namespace Turnos.Application.Turnos;

/// <summary>Validación de forma de <c>TurnoRequest</c>. La existencia y el estado
/// de baja de paciente/profesional se chequean en <c>TurnoService</c> (necesitan
/// la base).</summary>
public sealed class TurnoRequestValidator : AbstractValidator<TurnoRequest>
{
    public TurnoRequestValidator(IClock clock)
    {
        RuleFor(x => x.PacienteId).GreaterThan(0);

        RuleFor(x => x.ProfesionalId).GreaterThan(0);

        RuleFor(x => x.Inicio)
            .Must(TienePrecisionDeMinuto)
                .WithMessage("El inicio debe tener precisión de minuto (sin segundos).")
            .Must(inicio => inicio > clock.LocalNow)
                .WithMessage("El inicio debe ser a futuro.");

        RuleFor(x => x.Notas)
            .MaximumLength(500)
            .When(x => x.Notas is not null);
    }

    private static bool TienePrecisionDeMinuto(DateTime inicio) =>
        inicio.Second == 0 && inicio.Millisecond == 0 && inicio.Microsecond == 0;
}
