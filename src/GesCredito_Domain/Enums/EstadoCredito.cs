namespace GesCredito.Domain.Enums;

public enum EstadoCredito : byte
{
    Solicitado = 1,
    Aprobado = 2,
    Desembolsado = 3,
    Pagado = 4,
    Rechazado = 5,
    Cancelado = 6
}

public static class EstadoCreditoExtensions
{
    /// <summary>Estados terminales: el crédito ya no admite cambios.</summary>
    public static bool EsFinal(this EstadoCredito estado) =>
        estado is EstadoCredito.Pagado or EstadoCredito.Rechazado or EstadoCredito.Cancelado;

    /// <summary>Máquina de estados permitida.</summary>
    public static bool PuedeCambiarA(this EstadoCredito actual, EstadoCredito nuevo) =>
        actual == nuevo ||
        (actual, nuevo) switch
        {
            (EstadoCredito.Solicitado, EstadoCredito.Aprobado) => true,
            (EstadoCredito.Solicitado, EstadoCredito.Rechazado) => true,
            (EstadoCredito.Solicitado, EstadoCredito.Cancelado) => true,
            (EstadoCredito.Aprobado, EstadoCredito.Desembolsado) => true,
            (EstadoCredito.Aprobado, EstadoCredito.Cancelado) => true,
            (EstadoCredito.Desembolsado, EstadoCredito.Pagado) => true,
            _ => false
        };
}
