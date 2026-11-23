using System.ComponentModel.DataAnnotations;
using GesCredito.Domain.Enums;

namespace GesCredito.Services.Dtos;

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

// ---------- Clientes ----------
public class ClienteGuardarDto
{
    [Required, StringLength(20)] public string Identificacion { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Nombres { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellidos { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [StringLength(30)] public string? Telefono { get; set; }
    [StringLength(200)] public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
}

public record ClienteDto(
    int Id, string Identificacion, string Nombres, string Apellidos, string Email,
    string? Telefono, string? Direccion, DateTime FechaRegistro, bool Activo, int CantidadCreditos);

public record ClienteDetalleDto(
    int Id, string Identificacion, string Nombres, string Apellidos, string Email,
    string? Telefono, string? Direccion, DateTime FechaRegistro, bool Activo, int CantidadCreditos,
    IReadOnlyList<CreditoDto> Creditos);

// ---------- Créditos ----------
public class CreditoGuardarDto
{
    [Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [Range(typeof(decimal), "1", "999999999999")] public decimal Monto { get; set; }
    [Range(typeof(decimal), "0", "100")] public decimal TasaInteresAnual { get; set; }
    [Range(1, 360)] public int PlazoMeses { get; set; }
    /// <summary>Solo se usa al editar. En la creación siempre inicia como Solicitado.</summary>
    public EstadoCredito? Estado { get; set; }
    [StringLength(500)] public string? Observaciones { get; set; }
}

public record CreditoDto(
    int Id, string Numero, int ClienteId, string ClienteNombre, decimal Monto,
    decimal TasaInteresAnual, int PlazoMeses, decimal CuotaMensual, decimal TotalAPagar,
    EstadoCredito Estado, DateTime FechaSolicitud, DateTime? FechaDesembolso, string? Observaciones);
