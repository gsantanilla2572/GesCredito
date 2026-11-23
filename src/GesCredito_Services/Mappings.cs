using GesCredito.Domain.Entities;
using GesCredito.Services.Dtos;

namespace GesCredito.Services;

public static class Mappings
{
    public static ClienteDto ToDto(this Cliente c) => new(
        c.Id, c.Identificacion, c.Nombres, c.Apellidos, c.Email, c.Telefono, c.Direccion,
        c.FechaRegistro, c.Activo, c.Creditos.Count);

    public static ClienteDetalleDto ToDetalleDto(this Cliente c) => new(
        c.Id, c.Identificacion, c.Nombres, c.Apellidos, c.Email, c.Telefono, c.Direccion,
        c.FechaRegistro, c.Activo, c.Creditos.Count,
        c.Creditos.OrderByDescending(x => x.FechaSolicitud)
            .Select(x => x.ToDto(c.NombreCompleto)).ToList());

    public static CreditoDto ToDto(this Credito c) => c.ToDto(c.Cliente?.NombreCompleto ?? string.Empty);

    public static CreditoDto ToDto(this Credito c, string clienteNombre) => new(
        c.Id, c.Numero, c.ClienteId, clienteNombre, c.Monto, c.TasaInteresAnual, c.PlazoMeses,
        c.CuotaMensual, c.TotalAPagar, c.Estado, c.FechaSolicitud, c.FechaDesembolso, c.Observaciones);
}
