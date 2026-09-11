using Turnos.Application.Abstractions;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Tests.Fakes;

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;

    public int UsuarioId { get; set; } = 1;

    public Rol Rol { get; set; } = Rol.Admin;

    public int? ProfesionalId { get; set; }

    public static FakeCurrentUser Admin(int usuarioId = 1) =>
        new() { UsuarioId = usuarioId, Rol = Rol.Admin, ProfesionalId = null };

    public static FakeCurrentUser Profesional(int profesionalId, int usuarioId = 2) =>
        new() { UsuarioId = usuarioId, Rol = Rol.Profesional, ProfesionalId = profesionalId };
}
