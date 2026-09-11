using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = Turnos.Application.Common.Exceptions.ValidationException;

namespace Turnos.Api.Filters;

/// <summary>Corre el <c>IValidator&lt;T&gt;</c> de FluentValidation (si existe)
/// de cada argumento de la acción antes de ejecutarla, y agrupa los errores en
/// una sola <see cref="ValidationException"/> (→ 400 vía <c>ExceptionHandler</c>).
/// Reemplaza la validación automática de <c>[ApiController]</c>, desactivada en
/// <c>AddApi</c> (<c>SuppressModelStateInvalidFilter = true</c>) para que
/// validación de forma y de negocio compartan el mismo camino de error.
/// Registrado global en <c>AddControllers</c>.</summary>
internal sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errores = new Dictionary<string, string[]>();

        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argumento);
            var resultado = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            foreach (var grupo in resultado.Errors.GroupBy(e => e.PropertyName))
            {
                errores[grupo.Key] = grupo.Select(e => e.ErrorMessage).ToArray();
            }
        }

        if (errores.Count > 0)
        {
            throw new ValidationException(errores);
        }

        await next();
    }
}
