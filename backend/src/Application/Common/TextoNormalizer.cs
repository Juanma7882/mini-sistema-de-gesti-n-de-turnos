namespace Turnos.Application.Common;

/// <summary>Normalización de texto libre que entra por los <c>*Request</c>.</summary>
public static class TextoNormalizer
{
    /// <summary>
    /// Normaliza un nombre propio: recorta, colapsa espacios y deja cada palabra
    /// con la primera letra en mayúscula y el resto en minúscula
    /// (<c>"  maRÍa  JOSÉ "</c> → <c>"María José"</c>, <c>"jUAN"</c> → <c>"Juan"</c>).
    /// Respeta guiones y apóstrofos internos como separadores de palabra
    /// (<c>"maria-jose"</c> → <c>"Maria-Jose"</c>). Cultura invariante.
    /// </summary>
    public static string NombrePropio(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var palabras = value.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var i = 0; i < palabras.Length; i++)
        {
            palabras[i] = Capitalizar(palabras[i]);
        }

        return string.Join(' ', palabras);
    }

    private static string Capitalizar(string palabra)
    {
        var chars = palabra.ToCharArray();
        var inicioDePalabra = true;

        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = inicioDePalabra
                ? char.ToUpperInvariant(chars[i])
                : char.ToLowerInvariant(chars[i]);

            inicioDePalabra = chars[i] is '-' or '\'';
        }

        return new string(chars);
    }
}
