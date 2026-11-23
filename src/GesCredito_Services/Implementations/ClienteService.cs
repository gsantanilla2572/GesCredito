using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using GesCredito.Services.Dtos;
using GesCredito.Services.Exceptions;
using GesCredito.Services.Interfaces;

namespace GesCredito.Services.Implementations;

public class ClienteService(IClienteRepository clientes, IUnitOfWork uow) : IClienteService
{
    public async Task<PagedResult<ClienteDto>> ListarAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paginacion.Normalizar(page, pageSize);
        var (items, total) = await clientes.BuscarAsync(search, page, pageSize, ct);
        return new PagedResult<ClienteDto>(items.Select(c => c.ToDto()).ToList(), total, page, pageSize);
    }

    public async Task<ClienteDetalleDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var c = await clientes.GetByIdAsync(id, incluirCreditos: true, ct)
                ?? throw new NotFoundException($"No existe el cliente {id}.");
        return c.ToDetalleDto();
    }

    public async Task<ClienteDto> CrearAsync(ClienteGuardarDto dto, CancellationToken ct = default)
    {
        var identificacion = dto.Identificacion.Trim();
        if (await clientes.ExisteIdentificacionAsync(identificacion, null, ct))
            throw new ConflictException($"Ya existe un cliente con identificación {identificacion}.");

        var cliente = new Cliente { FechaRegistro = DateTime.UtcNow };
        Aplicar(cliente, dto);
        await clientes.AddAsync(cliente, ct);
        await uow.SaveChangesAsync(ct);
        return cliente.ToDto();
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteGuardarDto dto, CancellationToken ct = default)
    {
        var cliente = await clientes.GetByIdAsync(id, incluirCreditos: true, ct)
                      ?? throw new NotFoundException($"No existe el cliente {id}.");

        var identificacion = dto.Identificacion.Trim();
        if (await clientes.ExisteIdentificacionAsync(identificacion, id, ct))
            throw new ConflictException($"Ya existe un cliente con identificación {identificacion}.");

        Aplicar(cliente, dto);
        await uow.SaveChangesAsync(ct);
        return cliente.ToDto();
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var cliente = await clientes.GetByIdAsync(id, incluirCreditos: false, ct)
                      ?? throw new NotFoundException($"No existe el cliente {id}.");

        if (await clientes.TieneCreditosAsync(id, ct))
            throw new ConflictException(
                "El cliente tiene créditos asociados. Elimine sus créditos primero o márquelo como inactivo.");

        clientes.Remove(cliente);
        await uow.SaveChangesAsync(ct);
    }

    private static void Aplicar(Cliente c, ClienteGuardarDto dto)
    {
        c.Identificacion = dto.Identificacion.Trim();
        c.Nombres = dto.Nombres.Trim();
        c.Apellidos = dto.Apellidos.Trim();
        c.Email = dto.Email.Trim();
        c.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        c.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
        c.Activo = dto.Activo;
    }
}

internal static class Paginacion
{
    public static (int Page, int PageSize) Normalizar(int page, int pageSize) =>
        (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
