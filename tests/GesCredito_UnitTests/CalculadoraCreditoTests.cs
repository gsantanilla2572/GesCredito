using GesCredito.Domain.Services;

namespace GesCredito.UnitTests;

public class CalculadoraCreditoTests
{
    [Fact]
    public void TasaCero_DivideMontoEntrePlazo() =>
        Assert.Equal(100m, CalculadoraCredito.CalcularCuota(1200m, 0m, 12));

    [Fact]
    public void SistemaFrances_CuotaEsperada() =>
        // 10.000.000 al 12 % anual (1 % mensual) a 12 meses
        Assert.Equal(888487.89m, CalculadoraCredito.CalcularCuota(10_000_000m, 12m, 12));

    [Theory]
    [InlineData(0, 10, 12)]
    [InlineData(-5, 10, 12)]
    [InlineData(1000, 10, 0)]
    [InlineData(1000, -1, 12)]
    public void DatosInvalidos_Lanzan(decimal monto, decimal tasa, int plazo) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CalculadoraCredito.CalcularCuota(monto, tasa, plazo));
}
