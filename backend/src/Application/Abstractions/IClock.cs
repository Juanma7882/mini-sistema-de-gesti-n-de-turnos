namespace Turnos.Application.Abstractions;

/// <summary>Reloj inyectable. <see cref="UtcNow"/> para timestamps de auditoría;
/// <see cref="LocalNow"/> (hora local naïve de la clínica) para comparar contra
/// <c>Turno.Inicio</c>, que es <see cref="DateTimeKind.Unspecified"/>.</summary>
public interface IClock
{
    DateTime UtcNow { get; }

    DateTime LocalNow { get; }
}
