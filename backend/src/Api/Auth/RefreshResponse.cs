namespace Turnos.Api.Auth;

/// <summary>Body de <c>POST /api/auth/refresh</c>. El refresh token rotado va
/// solo en la cookie <c>rt</c> (<see cref="RefreshCookie"/>), nunca acá.</summary>
public sealed record RefreshResponse(string Token);
