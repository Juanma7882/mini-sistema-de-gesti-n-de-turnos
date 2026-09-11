namespace Turnos.Application.Common;

/// <summary>Parámetros de paginación de los listados. <see cref="Normalizado"/>
/// acota los valores recibidos por query string a un rango sano.</summary>
public sealed record PageRequest
{
    public const int PageSizeMaximo = 100;
    public const int PageSizePorDefecto = 20;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PageSizePorDefecto;

    /// <summary>Devuelve una copia con <c>Page &gt;= 1</c> y
    /// <c>1 &lt;= PageSize &lt;= <see cref="PageSizeMaximo"/></c>.</summary>
    public PageRequest Normalizado() => new()
    {
        Page = Page < 1 ? 1 : Page,
        PageSize = PageSize switch
        {
            < 1 => PageSizePorDefecto,
            > PageSizeMaximo => PageSizeMaximo,
            _ => PageSize,
        },
    };
}
