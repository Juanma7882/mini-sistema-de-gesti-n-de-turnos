using Turnos.Application.Abstractions;
using Turnos.Application.Common;
using Turnos.Application.Common.Exceptions;
using Turnos.Domain.Profesionales;
using Turnos.Domain.Usuarios;

namespace Turnos.Application.Profesionales;

/// <summary>Casos de uso del ABM de profesionales. En la Api, lectura para
/// cualquier autenticado y escritura solo Admin.</summary>
public sealed class ProfesionalService(
    IProfesionalRepository repository,
    IUsuarioRepository usuarios,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    public async Task<PagedResult<ProfesionalDto>> ListarAsync(
        string? search, PageRequest page, CancellationToken ct)
    {
        var p = page.Normalizado();
        var (items, total) = await repository.GetPagedAsync(search, p.Page, p.PageSize, ct);

        return new PagedResult<ProfesionalDto>
        {
            Items = items.Select(ProfesionalMapper.ToDto).ToList(),
            Total = total,
            Page = p.Page,
            PageSize = p.PageSize,
        };
    }

    public async Task<ProfesionalDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        return ProfesionalMapper.ToDto(profesional);
    }

    /// <summary>Crea el profesional junto con su cuenta de acceso: un profesional
    /// nunca existe sin usuario. <c>409</c> si el email ya está registrado.
    /// Un único <c>SaveChanges</c> (ambos repos comparten el <c>AppDbContext</c>
    /// scoped) persiste las dos entidades en una sola transacción; la FK
    /// <see cref="Profesional.UsuarioId"/> la resuelve el relationship fixup de
    /// EF Core a partir de la navegación <see cref="Profesional.Usuario"/>.</summary>
    public async Task<ProfesionalDto> CrearAsync(CrearProfesionalRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        if (await usuarios.GetByEmailAsync(email, ct) is not null)
        {
            throw new ConflictException("Ya existe un usuario registrado con ese email.");
        }

        var nombre = TextoNormalizer.NombrePropio(request.Nombre);
        var apellido = TextoNormalizer.NombrePropio(request.Apellido);

        var usuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Rol = Rol.Profesional,
            CreatedAt = clock.UtcNow,
        };

        var profesional = new Profesional
        {
            Especialidad = TextoNormalizer.TextoLibre(request.Especialidad),
            CreatedAt = clock.UtcNow,
            Usuario = usuario,
        };
        usuario.Profesional = profesional;

        await repository.AddAsync(profesional, ct);
        await usuarios.AddAsync(usuario, ct);
        await repository.SaveChangesAsync(ct);

        return ProfesionalMapper.ToDto(profesional);
    }

    public async Task<ProfesionalDto> EditarAsync(
        int id, ProfesionalRequest request, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        profesional.Usuario.Nombre = TextoNormalizer.NombrePropio(request.Nombre);
        profesional.Usuario.Apellido = TextoNormalizer.NombrePropio(request.Apellido);
        profesional.Especialidad = TextoNormalizer.TextoLibre(request.Especialidad);

        repository.Update(profesional);
        await repository.SaveChangesAsync(ct);

        return ProfesionalMapper.ToDto(profesional);
    }

    /// <summary>Baja lógica: deshabilita la cuenta del usuario asociado
    /// (<see cref="Usuario.DeletedAt"/>), lo que también le corta el login.
    /// <c>409</c> si el profesional tiene turnos activos
    /// (<c>Pendiente</c>/<c>Confirmado</c>).</summary>
    public async Task BajaAsync(int id, CancellationToken ct)
    {
        var profesional = await repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.Para("Profesional", id);

        if (await repository.TieneTurnosActivosAsync(id, ct))
        {
            throw new ConflictException(
                "El profesional tiene turnos activos y no puede darse de baja.");
        }

        profesional.Usuario.DeletedAt = clock.UtcNow;
        repository.Update(profesional);
        await repository.SaveChangesAsync(ct);
    }
}
