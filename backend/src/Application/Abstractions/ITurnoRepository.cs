using Turnos.Application.Turnos;
using Turnos.Domain.Turnos;

namespace Turnos.Application.Abstractions;

/// <summary>Acceso a datos del agregado Turno. Las lecturas traen
/// <c>Paciente</c> y <c>Profesional</c> con <c>Include</c> (una query, sin
/// N+1).</summary>
public interface ITurnoRepository
{
    /// <summary>Página de turnos que matchean <paramref name="filtro"/>. El
    /// alcance por rol ya viene resuelto en el filtro (el servicio setea
    /// <c>ProfesionalId</c> antes de llamar).</summary>
    Task<(IReadOnlyList<Turno> Items, int Total)> GetPagedAsync(
        TurnoFiltro filtro, int page, int pageSize, CancellationToken ct);

    /// <summary>Turno por id con <c>Paciente</c> y <c>Profesional</c> incluidos;
    /// <c>null</c> si no existe.</summary>
    Task<Turno?> GetByIdAsync(int id, CancellationToken ct);

    Task AddAsync(Turno turno, CancellationToken ct);

    void Update(Turno turno);

    /// <summary>Ya hay un turno no cancelado para ese profesional en ese
    /// instante. <paramref name="excluirTurnoId"/> se ignora a sí mismo al
    /// editar.</summary>
    Task<bool> ExisteSlotAsync(
        int profesionalId, DateTime inicio, int? excluirTurnoId, CancellationToken ct);

    /// <summary>Ese paciente ya tiene un turno <c>Pendiente</c> o
    /// <c>Confirmado</c> con ese profesional. <paramref name="excluirTurnoId"/>
    /// se ignora a sí mismo al editar. Lo respalda el índice único parcial
    /// <c>(ProfesionalId, PacienteId) WHERE Estado &lt; 2</c>.</summary>
    Task<bool> TienePacienteActivoConProfesionalAsync(
        int profesionalId, int pacienteId, int? excluirTurnoId, CancellationToken ct);

    /// <summary>Persiste. Traduce la violación del índice único parcial
    /// <c>(ProfesionalId, Inicio) WHERE Estado &lt;&gt; Cancelado</c> a
    /// <c>ConflictException</c> (carrera entre el chequeo y el insert).</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
