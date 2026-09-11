namespace Turnos.Application.Common;

/// <summary>Página de resultados de un listado. Serializa como
/// <c>{ items, total, page, pageSize }</c> (contrato del README).</summary>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Total de filas que matchean el filtro, sin paginar.</summary>
    public required int Total { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }
}
