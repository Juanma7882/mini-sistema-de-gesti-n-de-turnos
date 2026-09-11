using Turnos.Application.Abstractions;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Auth;

/// <summary>Casos de uso de autenticación: login, rotación de refresh token,
/// logout y <c>/auth/me</c>. Orquesta interfaces de Infrastructure
/// (<see cref="IPasswordHasher"/>, <see cref="IJwtTokenService"/>,
/// <see cref="IRefreshTokenService"/>).</summary>
public sealed class AuthService(
    IUsuarioRepository usuarios,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt,
    IRefreshTokenService refreshTokens,
    ICurrentUser currentUser)
{
    public async Task<AuthSession> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var usuario = await usuarios.GetByEmailAsync(request.Email.Trim(), ct);
        if (usuario is null || !passwordHasher.Verify(request.Password, usuario.PasswordHash))
        {
            throw CredencialesInvalidas();
        }

        var access = jwt.CreateAccessToken(usuario);
        var refresh = await refreshTokens.IssueAsync(usuario.Id, ct);

        return new AuthSession
        {
            Result = new AuthResultDto { Token = access, User = ToMeDto(usuario) },
            RefreshToken = refresh.RawToken,
            RefreshTokenExpiresAt = refresh.ExpiresAt,
        };
    }

    /// <summary>Valida y rota el refresh token (revoke-on-use) y emite un access
    /// token nuevo. <c>401</c> si el token falta, no existe, expiró o ya fue
    /// usado.</summary>
    public async Task<RefreshedSession> RefreshAsync(string? rawRefreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new UnauthorizedException("Falta el refresh token.");
        }

        var rotation = await refreshTokens.RotateAsync(rawRefreshToken, ct);

        var usuario = await usuarios.GetByIdAsync(rotation.UsuarioId, ct)
            ?? throw new UnauthorizedException("La cuenta ya no existe.");

        return new RefreshedSession
        {
            Token = jwt.CreateAccessToken(usuario),
            RefreshToken = rotation.RawToken,
            RefreshTokenExpiresAt = rotation.ExpiresAt,
        };
    }

    /// <summary>Revoca el refresh token actual. Idempotente: no falla si la
    /// cookie no vino o ya estaba revocada.</summary>
    public Task LogoutAsync(string? rawRefreshToken, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(rawRefreshToken)
            ? Task.CompletedTask
            : refreshTokens.RevokeAsync(rawRefreshToken, ct);

    public async Task<MeDto> MeAsync(CancellationToken ct)
    {
        var usuario = await usuarios.GetByIdAsync(currentUser.UsuarioId, ct)
            ?? throw new UnauthorizedException("La cuenta ya no existe.");

        return ToMeDto(usuario);
    }

    // Mensaje genérico a propósito: no revela si el email existe.
    private static UnauthorizedException CredencialesInvalidas() =>
        new("Email o contraseña incorrectos.");

    private static MeDto ToMeDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Email = usuario.Email,
        Role = usuario.Rol,
        ProfesionalId = usuario.ProfesionalId,
    };
}
