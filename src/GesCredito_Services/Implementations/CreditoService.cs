using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using GesCredito.Domain.Enums;
using GesCredito.Services.Dtos;
using GesCredito.Services.Exceptions;
using GesCredito.Services.Interfaces;

namespace GesCredito.Services.Implementations;

public class CreditoService(ICreditoRepository creditos, IClienteRepository clientes, IUnitOfWork uow) : ICreditoService
{
    public async Task<PagedResult<CreditoDto>> ListarAsync(
        int? clienteId, EstadoCredito? estado, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paginacion.Normalizar(page, pageSize);
        var (items, total) = await creditos.BuscarAsync(clienteId, estado, page, pageSize, ct);
        return new PagedResult<CreditoDto>(items.Select(c => c.ToDto()).ToList(), total, page, pageSize);
    }

    public async Task<CreditoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var c = await creditos.GetByIdAsync(id, ct) ?? throw new NotFoundException($"No existe el crédito {id}.");
        return c.ToDto();
    }

    public async Task<CreditoDto> CrearAsync(CreditoGuardarDto dto, CancellationToken ct = default)
    {
        var cliente = await ObtenerClienteValidoAsync(dto.ClienteId, ct);
        ValidarCondiciones(dto);

        var credito = new Credito
        {
            Numero = GenerarNumero(),
            ClienteId = cliente.Id,
            Cliente = cliente,
            Monto = dto.Monto,
            TasaInteresAnual = dto.TasaInteresAnual,
            PlazoMeses = dto.PlazoMeses,
            Estado = EstadoCredito.Solicitado,
            FechaSolicitud = DateTime.UtcNow,
            Observaciones = dto.Observaciones?.Trim()
        };
        credito.RecalcularCuota();

        await creditos.AddAsync(credito, ct);
        await uow.SaveChangesAsync(ct);
        return credito.ToDto();
    }

    public async Task<CreditoDto> ActualizarAsync(int id, CreditoGuardarDto dto, CancellationToken ct = default)
    {
        var credito = await creditos.GetByIdAsync(id, ct) ?? throw new NotFoundException($"No existe el crédito {id}.");

        if (credito.Estado.EsFinal())
            throw new BusinessRuleException($"El crédito {credito.Numero} está {credito.Estado} y no se puede modificar.");

        ValidarCondiciones(dto);

        var cambiaCondiciones = credito.Monto != dto.Monto
                                || credito.TasaInteresAnual != dto.TasaInteresAnual
                                || credito.PlazoMeses != dto.PlazoMeses
                                || credito.ClienteId != dto.ClienteId;

        if (cambiaCondiciones && credito.Estado != EstadoCredito.Solicitado)
            throw new BusinessRuleException(
                "Monto, tasa, plazo y cliente solo pueden modificarse mientras el crédito está en estado Solicitado.");

        if (credito.ClienteId != dto.ClienteId)
        {
            var nuevo = await ObtenerClienteValidoAsync(dto.ClienteId, ct);
            credito.ClienteId = nuevo.Id;
            credito.Cliente = nuevo;
        }

        var nuevoEstado = dto.Estado ?? credito.Estado;
        if (!credito.Estado.PuedeCambiarA(nuevoEstado))
            throw new BusinessRuleException($"No se puede pasar de {credito.Estado} a {nuevoEstado}.");

        if (nuevoEstado == EstadoCredito.Desembolsado && credito.Estado != EstadoCredito.Desembolsado)
            credito.FechaDesembolso = DateTime.UtcNow;

        credito.Estado = nuevoEstado;
        credito.Monto = dto.Monto;
        credito.TasaInteresAnual = dto.TasaInteresAnual;
        credito.PlazoMeses = dto.PlazoMeses;
        credito.Observaciones = dto.Observaciones?.Trim();
        credito.RecalcularCuota();

        await uow.SaveChangesAsync(ct);
        return credito.ToDto();
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var credito = await creditos.GetByIdAsync(id, ct) ?? throw new NotFoundException($"No existe el crédito {id}.");

        if (credito.Estado == EstadoCredito.Desembolsado)
            throw new BusinessRuleException("Un crédito desembolsado no se puede eliminar; debe pasar a Pagado.");

        creditos.Remove(credito);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Cliente> ObtenerClienteValidoAsync(int clienteId, CancellationToken ct)
    {
        var cliente = await clientes.GetByIdAsync(clienteId, false, ct)
                      ?? throw new NotFoundException($"No existe el cliente {clienteId}.");
        if (!cliente.Activo)
            throw new BusinessRuleException($"El cliente {cliente.NombreCompleto} está inactivo.");
        return cliente;
    }

    private static void ValidarCondiciones(CreditoGuardarDto dto)
    {
        if (dto.Monto <= 0) throw new BusinessRuleException("El monto debe ser mayor que 0.");
        if (dto.PlazoMeses is < 1 or > 360) throw new BusinessRuleException("El plazo debe estar entre 1 y 360 meses.");
        if (dto.TasaInteresAnual is < 0 or > 100) throw new BusinessRuleException("La tasa debe estar entre 0 y 100.");
    }

    private static string GenerarNumero() =>
        $"CR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}
