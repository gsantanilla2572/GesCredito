namespace GesCredito.Domain.Services;

public static class CalculadoraCredito
{
    /// <summary>Cuota mensual fija (sistema francés). Tasa anual en porcentaje (12 = 12 %).</summary>
    public static decimal CalcularCuota(decimal monto, decimal tasaInteresAnual, int plazoMeses)
    {
        if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor que 0.");
        if (plazoMeses <= 0) throw new ArgumentOutOfRangeException(nameof(plazoMeses), "El plazo debe ser mayor que 0.");
        if (tasaInteresAnual < 0) throw new ArgumentOutOfRangeException(nameof(tasaInteresAnual), "La tasa no puede ser negativa.");

        if (tasaInteresAnual == 0)
            return Math.Round(monto / plazoMeses, 2, MidpointRounding.AwayFromZero);

        var r = (double)tasaInteresAnual / 12d / 100d;
        var cuota = (double)monto * r / (1d - Math.Pow(1d + r, -plazoMeses));
        return Math.Round((decimal)cuota, 2, MidpointRounding.AwayFromZero);
    }
}
