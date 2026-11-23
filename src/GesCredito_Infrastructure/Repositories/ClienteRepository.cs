using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using GesCredito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesCredito.Infrastructure.Repositories;

public class ClienteRepository(GesCreditoDbContext db) : IClienteRepository
{
    public Task<Cliente?> GetByIdAsync(int id, bool incluirCreditos = false, CancellationToken ct = default)
    {
        IQueryable<Cliente> q = db.Clientes;
        if (incluirCreditos) q = q.Include(c => c.Creditos);
        return q.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<Cliente> q = db.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.Nombres.Contains(s) || c.Apellidos.Contains(s) ||
                             c.Identificacion.Contains(s) || c.Email.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .Include(c => c.Creditos)
            .OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId, CancellationToken ct = default) =>
        db.Clientes.AnyAsync(c => c.Identificacion == identificacion && (excluirId == null || c.Id != excluirId), ct);

    public Task<bool> TieneCreditosAsync(int clienteId, CancellationToken ct = default) =>
        db.Creditos.AnyAsync(c => c.ClienteId == clienteId, ct);

    public async Task AddAsync(Cliente cliente, CancellationToken ct = default) =>
        await db.Clientes.AddAsync(cliente, ct);

    public void Remove(Cliente cliente) => db.Clientes.Remove(cliente);
}
