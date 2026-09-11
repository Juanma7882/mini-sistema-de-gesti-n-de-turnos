using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Turnos.Application.Common.Exceptions;

namespace Turnos.Api.Errors;

/// <summary>Traduce las excepciones de negocio (subtipos de <c>DomainException</c>)
/// y cualquier excepción no controlada a <c>ProblemDetails</c> (RFC 7807).
/// Enganchado con <c>app.UseExceptionHandler()</c> en <c>Program.cs</c>; se
/// registra con <c>AddExceptionHandler&lt;ExceptionHandler&gt;()</c> +
/// <c>AddProblemDetails()</c> en <c>AddApi</c>.</summary>
internal sealed class ExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = Mapear(exception);
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        if (problemDetails.Status is null or StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado en {Path}", httpContext.Request.Path);
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private ProblemDetails Mapear(Exception exception) => exception switch
    {
        ValidationException ex => new ValidationProblemDetails(ex.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Title = "Una o más validaciones fallaron.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.io/400",
            Detail = ex.Message,
        },
        UnauthorizedException ex => Problema(StatusCodes.Status401Unauthorized, "No autorizado", ex.Message),
        ForbiddenException ex => Problema(StatusCodes.Status403Forbidden, "Prohibido", ex.Message),
        NotFoundException ex => Problema(StatusCodes.Status404NotFound, "No encontrado", ex.Message),
        ConflictException ex => Problema(StatusCodes.Status409Conflict, "Conflicto", ex.Message),
        _ => Problema(
            StatusCodes.Status500InternalServerError,
            "Error interno",
            environment.IsDevelopment() ? exception.Message : "Ocurrió un error inesperado."),
    };

    private static ProblemDetails Problema(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Type = $"https://httpstatuses.io/{status}",
    };
}
