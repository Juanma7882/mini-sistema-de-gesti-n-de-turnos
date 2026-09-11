namespace Turnos.Application.Abstractions;

/// <summary>Hashing de contraseñas. Implementación con BCrypt en Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
