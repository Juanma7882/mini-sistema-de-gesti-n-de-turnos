using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Turnos.Application.Auth;
using Turnos.Application.Pacientes;
using Turnos.Application.Profesionales;
using Turnos.Application.Turnos;

namespace Turnos.Application;

public static class DependencyInjection
{
    /// <summary>Registra los servicios de caso de uso y los validadores de
    /// FluentValidation del assembly. Las interfaces de infraestructura
    /// (<c>I*Repository</c>, <c>IClock</c>, <c>IJwtTokenService</c>…) las
    /// registra <c>AddInfrastructure</c>.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PacienteService>();
        services.AddScoped<ProfesionalService>();
        services.AddScoped<TurnoService>();
        services.AddScoped<AuthService>();

        services.AddValidatorsFromAssemblyContaining<PacienteRequestValidator>(
            ServiceLifetime.Scoped);

        return services;
    }
}
