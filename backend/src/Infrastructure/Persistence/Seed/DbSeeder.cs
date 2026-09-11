using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Turnos.Application.Abstractions;
using Turnos.Domain.Pacientes;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Turnos;
using Turnos.Domain.Usuarios;
using Turnos.Infrastructure.Auth;

namespace Turnos.Infrastructure.Persistence.Seed;

/// <summary>Siembra datos de demo si la base está vacía. Idempotente: si ya hay
/// usuarios, no hace nada. Lo llama la Api al arrancar (después de
/// <c>Migrate()</c>).</summary>
internal sealed class DbSeeder(
    AppDbContext db,
    IPasswordHasher hasher,
    IClock clock,
    IOptions<SeedOptions> seedOptions)
{
    private const string AdminPasswordFallback = "Admin123*";
    private const string ProfesionalPasswordFallback = "Profesional123*";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Usuarios.AnyAsync(ct))
        {
            return;
        }

        var now = clock.UtcNow;
        var options = seedOptions.Value;

        var profesionales = new List<Profesional>
        {
            new() { Nombre = "Laura", Apellido = "Gómez", Especialidad = "Clínica médica", CreatedAt = now },
            new() { Nombre = "Diego", Apellido = "Fernández", Especialidad = "Cardiología", CreatedAt = now },
            new() { Nombre = "Marta", Apellido = "Ruiz", Especialidad = "Pediatría", CreatedAt = now },
        };
        db.Profesionales.AddRange(profesionales);

        var pacientes = new List<Paciente>
        {
            new() { Nombre = "Juan", Apellido = "Pérez", Telefono = "1145670001", ObraSocial = "OSDE", CreatedAt = now },
            new() { Nombre = "Ana", Apellido = "López", Telefono = "1145670002", ObraSocial = "Swiss Medical", CreatedAt = now },
            new() { Nombre = "Carlos", Apellido = "Díaz", Telefono = "1145670003", ObraSocial = "Galeno", CreatedAt = now },
            new() { Nombre = "Sofía", Apellido = "Martínez", Telefono = "1145670004", ObraSocial = "PAMI", CreatedAt = now },
            new() { Nombre = "Lucía", Apellido = "Romero", Telefono = "1145670005", ObraSocial = "OSDE", CreatedAt = now },
        };
        db.Pacientes.AddRange(pacientes);

        await db.SaveChangesAsync(ct);

        db.Usuarios.AddRange(
            new Usuario
            {
                Nombre = "Administración",
                Email = "admin@clinica.test",
                PasswordHash = hasher.Hash(Fallback(options.AdminPassword, AdminPasswordFallback)),
                Rol = Rol.Admin,
                CreatedAt = now,
            },
            new Usuario
            {
                Nombre = "Laura Gómez",
                Email = "dra.gomez@clinica.test",
                PasswordHash = hasher.Hash(
                    Fallback(options.ProfessionalPassword, ProfesionalPasswordFallback)),
                Rol = Rol.Profesional,
                ProfesionalId = profesionales[0].Id,
                CreatedAt = now,
            });

        var baseDia = clock.LocalNow.Date.AddDays(1);
        db.Turnos.AddRange(
            NuevoTurno(pacientes[0], profesionales[0], baseDia.AddHours(9), EstadoTurno.Pendiente, now),
            NuevoTurno(pacientes[1], profesionales[0], baseDia.AddHours(10), EstadoTurno.Confirmado, now),
            NuevoTurno(pacientes[2], profesionales[1], baseDia.AddHours(9).AddMinutes(30), EstadoTurno.Pendiente, now),
            NuevoTurno(pacientes[3], profesionales[2], baseDia.AddDays(1).AddHours(11), EstadoTurno.Pendiente, now),
            NuevoTurno(pacientes[0], profesionales[1], baseDia.AddDays(-8).AddHours(15), EstadoTurno.Atendido, now),
            NuevoTurno(pacientes[4], profesionales[0], baseDia.AddDays(-3).AddHours(12), EstadoTurno.Cancelado, now));

        await db.SaveChangesAsync(ct);
    }

    private static Turno NuevoTurno(
        Paciente paciente, Profesional profesional, DateTime inicio, EstadoTurno estado, DateTime now) => new()
    {
        PacienteId = paciente.Id,
        ProfesionalId = profesional.Id,
        Inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Unspecified),
        Estado = estado,
        CreatedAt = now,
        UpdatedAt = now,
    };

    private static string Fallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
