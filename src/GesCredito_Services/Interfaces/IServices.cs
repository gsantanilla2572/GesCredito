using GesCredito.Domain.Enums;
using GesCredito.Services.Dtos;

namespace GesCredito.Services.Interfaces;

public interface IClienteService
{
    Task<PagedResult<ClienteDto>> ListarAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<ClienteDetalleDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<ClienteDto> CrearAsync(ClienteGuardarDto dto, CancellationToken ct = default);
    Task<ClienteDto> ActualizarAsync(int id, ClienteGuardarDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}

public interface ICreditoService
{
    Task<PagedResult<CreditoDto>> ListarAsync(int? clienteId, EstadoCredito? estado, int page, int pageSize, CancellationToken ct = default);
    Task<CreditoDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<CreditoDto> CrearAsync(CreditoGuardarDto dto, CancellationToken ct = default);
    Task<CreditoDto> ActualizarAsync(int id, CreditoGuardarDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
