using System.Net.Http.Headers;
using System.Net.Http.Json;
using Turnos.Application.Auth;

namespace Turnos.Api.Tests.Support;

/// <summary>Cliente HTTP ya logueado: <see cref="Http"/> trae el header
/// <c>Authorization: Bearer</c> puesto por defecto. <see cref="RefreshToken"/>
/// es el valor crudo de la cookie <c>rt</c> — el <see cref="HttpClient"/> de test
/// no es un browser, no la persiste sola entre requests (ver
/// <see cref="HttpRequestExtensions.WithRefreshCookie"/>).</summary>
public sealed class AuthenticatedClient(HttpClient http, string token, string refreshToken) : IDisposable
{
    public HttpClient Http { get; } = http;

    public string Token { get; } = token;

    public string RefreshToken { get; } = refreshToken;

    public void Dispose() => Http.Dispose();
}

public static class LoginHelper
{
    public const string AdminEmail = "admin@clinica.test";
    public const string AdminPassword = "Test-Admin-123*";
    public const string ProfesionalEmail = "dra.gomez@clinica.test";
    public const string ProfesionalPassword = "Test-Profesional-123*";

    public static Task<AuthenticatedClient> AsAdminAsync(this IntegrationTestFactory factory) =>
        LoginAsync(factory, AdminEmail, AdminPassword);

    public static Task<AuthenticatedClient> AsProfesionalAsync(this IntegrationTestFactory factory) =>
        LoginAsync(factory, ProfesionalEmail, ProfesionalPassword);

    public static async Task<AuthenticatedClient> LoginAsync(
        IntegrationTestFactory factory, string email, string password)
    {
        using var anonimo = factory.CreateClient();
        var response = await anonimo.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest { Email = email, Password = password }, ApiJson.Options);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResultDto>(ApiJson.Options);
        var rawRefreshToken = ExtraerCookie(response, "rt")
            ?? throw new InvalidOperationException("El login no devolvió la cookie 'rt'.");

        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        return new AuthenticatedClient(http, body.Token, rawRefreshToken);
    }

    /// <summary>Extrae el valor crudo de una cookie del header <c>Set-Cookie</c>
    /// de una respuesta (formato <c>nombre=valor; attr1; attr2=...</c>).</summary>
    public static string? ExtraerCookie(HttpResponseMessage response, string nombre)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var prefijo = $"{nombre}=";
        foreach (var cookie in cookies)
        {
            if (!cookie.StartsWith(prefijo, StringComparison.Ordinal))
            {
                continue;
            }

            var valor = cookie[prefijo.Length..];
            var finDelValor = valor.IndexOf(';');

            return finDelValor >= 0 ? valor[..finDelValor] : valor;
        }

        return null;
    }
}
