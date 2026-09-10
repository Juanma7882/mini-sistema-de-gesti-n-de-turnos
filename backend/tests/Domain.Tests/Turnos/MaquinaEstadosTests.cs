using FluentAssertions;
using Turnos.Domain.Turnos;

namespace Turnos.Domain.Tests.Turnos;

public class MaquinaEstadosTests
{
    private static readonly EstadoTurno[] TodosLosEstados = Enum.GetValues<EstadoTurno>();

    // Las 4 flechas del diagrama del README.
    public static TheoryData<EstadoTurno, EstadoTurno> FlechasLegales() => new()
    {
        { EstadoTurno.Pendiente, EstadoTurno.Confirmado },
        { EstadoTurno.Pendiente, EstadoTurno.Cancelado },
        { EstadoTurno.Confirmado, EstadoTurno.Atendido },
        { EstadoTurno.Confirmado, EstadoTurno.Cancelado },
    };

    // Los 12 pares ordenados restantes (16 - 4): identidad, salto, retrocesos y salidas desde terminales.
    public static TheoryData<EstadoTurno, EstadoTurno> FlechasIlegales()
    {
        var legales = FlechasLegales().Select(row => ((EstadoTurno)row[0]!, (EstadoTurno)row[1]!)).ToHashSet();
        var data = new TheoryData<EstadoTurno, EstadoTurno>();
        foreach (var desde in TodosLosEstados)
        foreach (var hacia in TodosLosEstados)
        {
            if (!legales.Contains((desde, hacia)))
            {
                data.Add(desde, hacia);
            }
        }

        return data;
    }

    public static TheoryData<EstadoTurno, EstadoTurno> TodosLosPares()
    {
        var data = new TheoryData<EstadoTurno, EstadoTurno>();
        foreach (var desde in TodosLosEstados)
        foreach (var hacia in TodosLosEstados)
        {
            data.Add(desde, hacia);
        }

        return data;
    }

    public static TheoryData<EstadoTurno> CadaEstado()
    {
        var data = new TheoryData<EstadoTurno>();
        foreach (var estado in TodosLosEstados)
        {
            data.Add(estado);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(FlechasLegales))]
    public void EsTransicionValida_FlechaLegal_DevuelveTrue(EstadoTurno desde, EstadoTurno hacia)
    {
        MaquinaEstados.EsTransicionValida(desde, hacia).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(FlechasIlegales))]
    public void EsTransicionValida_FlechaIlegal_DevuelveFalse(EstadoTurno desde, EstadoTurno hacia)
    {
        MaquinaEstados.EsTransicionValida(desde, hacia).Should().BeFalse();
    }

    [Fact]
    public void TransicionesLegales_DesdePendiente_EsConfirmadoYCancelado()
    {
        MaquinaEstados.TransicionesLegales(EstadoTurno.Pendiente)
            .Should().BeEquivalentTo(new[] { EstadoTurno.Confirmado, EstadoTurno.Cancelado });
    }

    [Fact]
    public void TransicionesLegales_DesdeConfirmado_EsAtendidoYCancelado()
    {
        MaquinaEstados.TransicionesLegales(EstadoTurno.Confirmado)
            .Should().BeEquivalentTo(new[] { EstadoTurno.Atendido, EstadoTurno.Cancelado });
    }

    [Theory]
    [InlineData(EstadoTurno.Cancelado)]
    [InlineData(EstadoTurno.Atendido)]
    public void TransicionesLegales_DesdeEstadoTerminal_EsVacio(EstadoTurno terminal)
    {
        MaquinaEstados.TransicionesLegales(terminal).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(CadaEstado))]
    public void TransicionesLegales_ParaCualquierEstado_NoSeIncluyeASiMismo(EstadoTurno estado)
    {
        MaquinaEstados.TransicionesLegales(estado).Should().NotContain(estado);
    }

    [Theory]
    [MemberData(nameof(TodosLosPares))]
    public void EsTransicionValida_EsCoherenteCon_TransicionesLegales(EstadoTurno desde, EstadoTurno hacia)
    {
        MaquinaEstados.EsTransicionValida(desde, hacia)
            .Should().Be(MaquinaEstados.TransicionesLegales(desde).Contains(hacia));
    }

    [Fact]
    public void TransicionesLegales_ValorFueraDeRango_DevuelveVacio()
    {
        MaquinaEstados.TransicionesLegales((EstadoTurno)99).Should().BeEmpty();
    }
}
