namespace Turnos.Api.Tests.Support;

public static class HttpRequestExtensions
{
    /// <summary>Adjunta la cookie <c>rt</c> como header <c>Cookie</c> — el
    /// <see cref="HttpClient"/> de test no es un browser, no la reenvía sola.</summary>
    public static HttpRequestMessage WithRefreshCookie(this HttpRequestMessage request, string rawToken)
    {
        request.Headers.Add("Cookie", $"rt={rawToken}");

        return request;
    }
}
