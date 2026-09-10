namespace Turnos.Domain.Usuarios;

/// <summary>
/// Rol de la cuenta de acceso. Se persiste como <c>int</c>; viaja como string en el JSON.
/// </summary>
public enum Rol
{
    Admin = 0,
    Profesional = 1,
}
