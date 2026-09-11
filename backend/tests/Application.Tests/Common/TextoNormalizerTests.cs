using FluentAssertions;
using Turnos.Application.Common;

namespace Turnos.Application.Tests.Common;

public class TextoNormalizerTests
{
    [Theory]
    [InlineData("jUAN", "Juan")]
    [InlineData("  maRÍa  JOSÉ ", "María José")]
    [InlineData("gonzález", "González")]
    [InlineData("DE LA CRUZ", "De La Cruz")]
    [InlineData("maria-jose", "Maria-Jose")]
    [InlineData("o'brien", "O'Brien")]
    public void NombrePropio_NormalizaCadaPalabra(string entrada, string esperado) =>
        TextoNormalizer.NombrePropio(entrada).Should().Be(esperado);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NombrePropio_VacioONull_DevuelveCadenaVacia(string? entrada) =>
        TextoNormalizer.NombrePropio(entrada).Should().BeEmpty();

    [Theory]
    [InlineData("cardiologia", "Cardiologia")]
    [InlineData("CARDIOLOGIA", "Cardiologia")]
    [InlineData("  swiss medical  ", "Swiss Medical")]
    [InlineData("Pediatría", "Pediatría")]
    public void TextoLibre_CapitalizaCadaPalabra(string entrada, string esperado) =>
        TextoNormalizer.TextoLibre(entrada).Should().Be(esperado);

    [Theory]
    [InlineData("OSDE")]
    [InlineData("PAMI")]
    [InlineData("IOMA")]
    public void TextoLibre_RespetaSiglasEnMayusculas(string entrada) =>
        TextoNormalizer.TextoLibre(entrada).Should().Be(entrada);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TextoLibre_VacioONull_DevuelveCadenaVacia(string? entrada) =>
        TextoNormalizer.TextoLibre(entrada).Should().BeEmpty();
}
