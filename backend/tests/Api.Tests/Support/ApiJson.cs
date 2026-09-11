using System.Text.Json;
using System.Text.Json.Serialization;

namespace Turnos.Api.Tests.Support;

/// <summary>Mismas opciones de JSON que usa la Api (<c>AddApi</c>, §6.2): enums
/// como string. Sin esto, cualquier DTO con un enum (<c>Rol</c>,
/// <c>EstadoTurno</c>) falla al leer o escribir del lado del test —
/// <c>System.Net.Http.Json</c> usa enums numéricos por default si no se le
/// pasan las mismas opciones que configuró el servidor.</summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
