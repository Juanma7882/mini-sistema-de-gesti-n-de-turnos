using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Turnos.Api.Auth;
using Turnos.Api.Errors;
using Turnos.Api.Filters;
using Turnos.Api.Realtime;
using Turnos.Application.Abstractions;
using Turnos.Application.Turnos;
using Turnos.Infrastructure.Auth;

namespace Turnos.Api;

/// <summary>Composición de la capa Api: controladores + filtro de validación,
/// autenticación/autorización JWT, CORS, manejo de errores y Swagger. La llama
/// <c>Program.cs</c> después de <c>AddApplication</c> e
/// <c>AddInfrastructure</c>.</summary>
public static class DependencyInjection
{
    public const string FrontendCorsPolicy = "frontend";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ValidationFilter>();

        // [Route("api/[controller]")] con LowercaseUrls: AuthController -> /api/auth,
        // PacientesController -> /api/pacientes, etc. (rutas del README en minúscula).
        services.AddRouting(options => options.LowercaseUrls = true);

        services
            .AddControllers(options => options.Filters.Add<ValidationFilter>())
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(options =>
                // El 400 de validación lo arma ValidationFilter con el mismo
                // formato que el resto de los errores de negocio (ExceptionHandler).
                options.SuppressModelStateInvalidFilter = true);

        services.AddExceptionHandler<ExceptionHandler>();
        services.AddProblemDetails();

        AddJwtAuth(services, configuration);
        AddSwagger(services);
        AddFrontendCors(services, configuration);

        services.AddSignalR();
        services.AddScoped<ITurnoNotifier, SignalRTurnoNotifier>();

        return services;
    }

    private static void AddJwtAuth(IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // JwtTokenService emite los claims "sub"/"role" tal cual. Sin
                // MapInboundClaims = false, ASP.NET Core los remapea a URIs de
                // esquema y rompe [Authorize(Roles = ...)] y la lectura de "sub"
                // en CurrentUser.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                };

                // El WebSocket nativo del browser no puede mandar el header
                // Authorization: SignalR manda el access token por query string
                // en su lugar, así que hay que aceptarlo ahí solo para /hubs.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Turnos API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Access token. Ejemplo: \"Bearer {token}\"",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    Array.Empty<string>()
                },
            });
        });
    }

    private static void AddFrontendCors(IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration["Cors:AllowedOrigins"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length == 0)
        {
            // Default de dev: el frontend Vite corre acá si no se configuró nada.
            origins = ["http://localhost:5173"];
        }

        services.AddCors(options => options.AddPolicy(
            FrontendCorsPolicy,
            policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
    }
}
