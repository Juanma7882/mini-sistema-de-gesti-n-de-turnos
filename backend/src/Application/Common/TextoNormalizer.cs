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

    /// <summary>
    /// Normaliza texto libre de catálogo abierto (especialidad, obra social):
    /// recorta, colapsa espacios y capitaliza cada palabra
    /// (<c>"cardiologia"</c> → <c>"Cardiologia"</c>, <c>"swiss medical"</c> →
    /// <c>"Swiss Medical"</c>) — salvo que la palabra sea corta (2-5 letras) y
    /// venga completamente en mayúsculas, en cuyo caso se respeta tal cual por
    /// ser probablemente una sigla real (<c>"OSDE"</c>, <c>"PAMI"</c>,
    /// <c>"IOMA"</c>); una palabra larga en mayúsculas (<c>"CARDIOLOGIA"</c>) sí
    /// se capitaliza, porque es más probable que sea un descuido de mayúsculas
    /// que una sigla. Reduce duplicados por variación de mayúsculas/minúsculas,
    /// pero no corrige ortografía ni tildes: sin un catálogo con FK (ver
    /// <c>mejoras-futuras.md</c>), dos formas distintas de escribir lo mismo
    /// pueden seguir generando valores diferentes.
    /// </summary>
    public static string TextoLibre(string? value)
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
            palabras[i] = EsProbablementeSigla(palabras[i]) ? palabras[i] : Capitalizar(palabras[i]);
        }

        return string.Join(' ', palabras);
    }

    private static bool EsProbablementeSigla(string palabra) =>
        palabra.Length is > 1 and <= 5 && palabra.All(c => !char.IsLetter(c) || char.IsUpper(c));
}
