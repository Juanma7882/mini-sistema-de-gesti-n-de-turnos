using System.Net.Http.Json;
using Turnos.Application.Pacientes;
using Turnos.Application.Profesionales;
using Turnos.Application.Turnos;

namespace Turnos.Api.Tests.Support;

/// <summary>Arma datos propios para cada test (no depende de los IDs/filas
/// exactas del seed, que otras clases de la misma colección también pueden
/// estar leyendo o modificando).</summary>
public static class ApiTestDataExtensions
{
    private static int _minutosOffset;

    public static async Task<PacienteDto> CrearPacienteAsync(this HttpClient admin, string? sufijo = null)
    {
        sufijo ??= Guid.NewGuid().ToString("N")[..8];
        var response = await admin.PostAsJsonAsync(
            "/api/pacientes",
            new PacienteRequest
            {
                Nombre = "Test",
                Apellido = $"Paciente-{sufijo}",
                Telefono = "1100000000",
                ObraSocial = "OSDE",
            },
            ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PacienteDto>(ApiJson.Options))!;
    }

    public static async Task<ProfesionalDto> CrearProfesionalAsync(this HttpClient admin, string? sufijo = null)
    {
        sufijo ??= Guid.NewGuid().ToString("N")[..8];
        var response = await admin.PostAsJsonAsync(
            "/api/profesionales",
            new CrearProfesionalRequest
            {
                Nombre = "Test",
                Apellido = $"Profesional-{sufijo}",
                Especialidad = "Clínica médica",
                Email = $"profesional-{sufijo}@test.local",
                Password = "Password123*",
            },
            ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProfesionalDto>(ApiJson.Options))!;
    }

    public static async Task<TurnoDto> CrearTurnoAsync(
        this HttpClient admin, int pacienteId, int profesionalId, DateTime inicio, string? notas = null)
    {
        var response = await admin.PostAsJsonAsync(
            "/api/turnos",
            new TurnoRequest
            {
                PacienteId = pacienteId,
                ProfesionalId = profesionalId,
                Inicio = inicio,
                Notas = notas,
            },
            ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TurnoDto>(ApiJson.Options))!;
    }

    /// <summary>Un horario a futuro (año 2027), precisión de minuto, único en
    /// cada llamada — evita choques de slot entre tests que comparten la DB de
    /// la colección. Para el test que necesita el MISMO slot dos veces (anti
    /// doble-turno) hay que llamar esto una sola vez y reusar el valor.</summary>
    public static DateTime SlotFuturoUnico()
    {
        var minutos = Interlocked.Increment(ref _minutosOffset);

        return new DateTime(2027, 1, 1, 8, 0, 0, DateTimeKind.Unspecified).AddMinutes(minutos);
    }
}
