using GesCredito.Domain.Enums;
using GesCredito.Domain.Services;

namespace GesCredito.Domain.Entities;

public class Credito
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public decimal Monto { get; set; }
    public decimal TasaInteresAnual { get; set; }
    public int PlazoMeses { get; set; }
    public decimal CuotaMensual { get; set; }
    public EstadoCredito Estado { get; set; } = EstadoCredito.Solicitado;

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaDesembolso { get; set; }
    public string? Observaciones { get; set; }

    public decimal TotalAPagar => CuotaMensual * PlazoMeses;

    public void RecalcularCuota() =>
        CuotaMensual = CalculadoraCredito.CalcularCuota(Monto, TasaInteresAnual, PlazoMeses);
}
