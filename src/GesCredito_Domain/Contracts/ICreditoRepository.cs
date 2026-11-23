using GesCredito.Domain.Entities;
using GesCredito.Domain.Enums;

namespace GesCredito.Domain.Contracts;

public interface ICreditoRepository
{
    Task<Credito?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<Credito> Items, int Total)> BuscarAsync(int? clienteId, EstadoCredito? estado, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Credito credito, CancellationToken ct = default);
    void Remove(Credito credito);
}
