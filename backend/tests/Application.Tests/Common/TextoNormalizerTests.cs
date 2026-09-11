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
}
