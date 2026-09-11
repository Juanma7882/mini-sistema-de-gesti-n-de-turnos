namespace Turnos.Api.Auth;

/// <summary>Cookie <c>httpOnly</c> del refresh token (<c>rt</c>). <c>Path</c>
/// cubre todo <c>/api/auth</c> y no solo <c>/api/auth/refresh</c>: <c>logout</c>
/// también necesita leerla para revocar el token actual, y con un <c>Path</c>
/// más angosto el navegador no la manda ahí. <c>SameSite=None</c> porque
/// frontend (Vercel) y backend (Railway) son orígenes distintos — el spec exige
/// <c>Secure</c> junto con <c>None</c>, así que en dev hay que levantar la Api
/// por https (perfil <c>https</c> de <c>launchSettings.json</c>).</summary>
internal static class RefreshCookie
{
    private const string Name = "rt";
    private const string CookiePath = "/api/auth";

    public static void Append(HttpResponse response, string rawToken, DateTime expiresAtUtc)
    {
        var options = BaseOptions();
        options.Expires = new DateTimeOffset(expiresAtUtc, TimeSpan.Zero);

        response.Cookies.Append(Name, rawToken, options);
    }

    public static void Delete(HttpResponse response) => response.Cookies.Delete(Name, BaseOptions());

    public static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out var value) ? value : null;

    private static CookieOptions BaseOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = CookiePath,
    };
}
