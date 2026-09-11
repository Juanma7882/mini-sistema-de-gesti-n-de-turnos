using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Turnos.Api.Tests.Support;
using Turnos.Application.Auth;
using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Api.Tests.Turnos;

[Collection(ApiCollection.Name)]
public sealed class TransicionesEstadoTests(IntegrationTestFactory factory)
{
    /// <summary>Las 4 flechas legales del diagrama del README + 4 casos
    /// ilegales representativos (salto de paso, no-op, y reabrir cada estado
    /// terminal). No es una tabla combinatoria completa (4×4): esto prueba que
    /// la Api respeta la máquina de estados de punta a punta, no re-verifica la
    /// máquina en sí (ya cubierta exhaustivamente en <c>Domain.Tests</c>).</summary>
    public static IEnumerable<object[]> Transiciones()
    {
        (EstadoTurno Desde, EstadoTurno Hacia, bool EsLegal)[] casos =
        [
            (EstadoTurno.Pendiente, EstadoTurno.Confirmado, true),
            (EstadoTurno.Pendiente, EstadoTurno.Cancelado, true),
            (EstadoTurno.Confirmado, EstadoTurno.Atendido, true),
            (EstadoTurno.Confirmado, EstadoTurno.Cancelado, true),
            (EstadoTurno.Pendiente, EstadoTurno.Atendido, false),
            (EstadoTurno.Pendiente, EstadoTurno.Pendiente, false),
            (EstadoTurno.Cancelado, EstadoTurno.Confirmado, false),
            (EstadoTurno.Atendido, EstadoTurno.Pendiente, false),
        ];

        foreach (var (desde, hacia, esLegal) in casos)
        {
            yield return [desde, hacia, esLegal];
        }
    }

    [Theory]
    [MemberData(nameof(Transiciones))]
    public async Task ComoAdmin_RespetaLaMaquinaDeEstados(EstadoTurno desde, EstadoTurno hacia, bool esLegal)
    {
        using var admin = await factory.AsAdminAsync();
        var turno = await CrearTurnoEnEstadoAsync(admin.Http, desde);

        var response = await admin.Http.PatchAsJsonAsync(
            $"/api/turnos/{turno.Id}/estado", new CambiarEstadoRequest { Estado = hacia }, ApiJson.Options);

        response.StatusCode.Should().Be(esLegal ? HttpStatusCode.OK : HttpStatusCode.Conflict);
    }

    [Theory]
    [MemberData(nameof(Transiciones))]
    public async Task ComoProfesionalDueño_RespetaLaMaquinaDeEstados(
        EstadoTurno desde, EstadoTurno hacia, bool esLegal)
    {
        using var admin = await factory.AsAdminAsync();
        using var profesional = await factory.AsProfesionalAsync();

        var me = await profesional.Http.GetFromJsonAsync<MeDto>("/api/auth/me", ApiJson.Options);
        var miProfesionalId = me!.ProfesionalId!.Value;

        var turno = await CrearTurnoEnEstadoAsync(admin.Http, desde, miProfesionalId);

        var response = await profesional.Http.PatchAsJsonAsync(
            $"/api/turnos/{turno.Id}/estado", new CambiarEstadoRequest { Estado = hacia }, ApiJson.Options);

        response.StatusCode.Should().Be(esLegal ? HttpStatusCode.OK : HttpStatusCode.Conflict);
    }

    /// <summary>Crea un turno y lo hace transitar por el camino legal más corto
    /// hasta <paramref name="estado"/> (todos con Admin, que puede disparar
    /// cualquier flecha legal), para arrancar cada caso desde el estado que
    /// pide la matriz.</summary>
    private static async Task<TurnoDto> CrearTurnoEnEstadoAsync(
        HttpClient admin, EstadoTurno estado, int? profesionalId = null)
    {
        var paciente = await admin.CrearPacienteAsync();
        var pid = profesionalId ?? (await admin.CrearProfesionalAsync()).Id;
        var turno = await admin.CrearTurnoAsync(paciente.Id, pid, ApiTestDataExtensions.SlotFuturoUnico());

        foreach (var paso in CaminoHasta(estado))
        {
            var response = await admin.PatchAsJsonAsync(
                $"/api/turnos/{turno.Id}/estado", new CambiarEstadoRequest { Estado = paso }, ApiJson.Options);
            response.EnsureSuccessStatusCode();
        }

        return estado == EstadoTurno.Pendiente
            ? turno
            : (await admin.GetFromJsonAsync<TurnoDto>($"/api/turnos/{turno.Id}", ApiJson.Options))!;
    }

    private static IEnumerable<EstadoTurno> CaminoHasta(EstadoTurno estado) => estado switch
    {
        EstadoTurno.Pendiente => [],
        EstadoTurno.Confirmado => [EstadoTurno.Confirmado],
        EstadoTurno.Cancelado => [EstadoTurno.Cancelado],
        EstadoTurno.Atendido => [EstadoTurno.Confirmado, EstadoTurno.Atendido],
        _ => throw new ArgumentOutOfRangeException(nameof(estado)),
    };
}
