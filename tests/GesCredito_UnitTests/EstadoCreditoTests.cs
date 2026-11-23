using GesCredito.Domain.Enums;

namespace GesCredito.UnitTests;

public class EstadoCreditoTests
{
    [Theory]
    [InlineData(EstadoCredito.Solicitado, EstadoCredito.Aprobado, true)]
    [InlineData(EstadoCredito.Solicitado, EstadoCredito.Rechazado, true)]
    [InlineData(EstadoCredito.Aprobado, EstadoCredito.Desembolsado, true)]
    [InlineData(EstadoCredito.Desembolsado, EstadoCredito.Pagado, true)]
    [InlineData(EstadoCredito.Solicitado, EstadoCredito.Desembolsado, false)]
    [InlineData(EstadoCredito.Desembolsado, EstadoCredito.Cancelado, false)]
    [InlineData(EstadoCredito.Pagado, EstadoCredito.Solicitado, false)]
    [InlineData(EstadoCredito.Aprobado, EstadoCredito.Aprobado, true)]
    public void Transiciones(EstadoCredito desde, EstadoCredito hacia, bool permitido) =>
        Assert.Equal(permitido, desde.PuedeCambiarA(hacia));

    [Theory]
    [InlineData(EstadoCredito.Pagado, true)]
    [InlineData(EstadoCredito.Rechazado, true)]
    [InlineData(EstadoCredito.Cancelado, true)]
    [InlineData(EstadoCredito.Solicitado, false)]
    [InlineData(EstadoCredito.Desembolsado, false)]
    public void EsFinal(EstadoCredito estado, bool esperado) => Assert.Equal(esperado, estado.EsFinal());
}
