using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests;

/// <summary>Fábricas de entidades para armar escenarios de test.</summary>
internal static class TestData
{
    public static Paciente PacienteActivo(int id, string nombre = "Ana", string apellido = "Diaz") => new()
    {
        Id = id,
        Nombre = nombre,
        Apellido = apellido,
        Telefono = "1122334455",
        ObraSocial = "OSDE",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    public static Paciente PacienteDadoDeBaja(int id) => new()
    {
        Id = id,
        Nombre = "Baja",
        Apellido = "Baja",
        Telefono = "0",
        ObraSocial = "-",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        DeletedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    public static Profesional ProfesionalActivo(int id, string nombre = "Laura", string apellido = "Gomez")
    {
        var usuario = new Usuario
        {
            Id = id,
            Nombre = nombre,
            Apellido = apellido,
            Email = $"{nombre}.{apellido}@clinica.test".ToLowerInvariant(),
            PasswordHash = "hash:secret",
            Rol = Rol.Profesional,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        var profesional = new Profesional
        {
            Id = id,
            Especialidad = "Clinica",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Usuario = usuario,
        };
        usuario.Profesional = profesional;
        return profesional;
    }

    public static Turno Turno(
        int id,
        int pacienteId,
        int profesionalId,
        DateTime inicio,
        EstadoTurno estado = EstadoTurno.Pendiente) => new()
    {
        Id = id,
        PacienteId = pacienteId,
        ProfesionalId = profesionalId,
        Inicio = inicio,
        Estado = estado,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    public static Usuario Admin(int id = 1, string email = "admin@clinica.test") => new()
    {
        Id = id,
        Nombre = "Admin",
        Apellido = string.Empty,
        Email = email,
        PasswordHash = "hash:secret",
        Rol = Rol.Admin,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    public static Usuario Profesional(int id, int profesionalId, string email)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nombre = "Profesional",
            Apellido = "Apellido",
            Email = email,
            PasswordHash = "hash:secret",
            Rol = Rol.Profesional,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        usuario.Profesional = new Profesional
        {
            Id = profesionalId,
            Especialidad = "Clinica",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Usuario = usuario,
        };
        return usuario;
    }
}
