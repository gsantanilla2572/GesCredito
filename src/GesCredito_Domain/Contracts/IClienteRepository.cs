using GesCredito.Domain.Entities;

namespace GesCredito.Domain.Contracts;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(int id, bool incluirCreditos = false, CancellationToken ct = default);
    Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId, CancellationToken ct = default);
    Task<bool> TieneCreditosAsync(int clienteId, CancellationToken ct = default);
    Task AddAsync(Cliente cliente, CancellationToken ct = default);
    void Remove(Cliente cliente);
}
