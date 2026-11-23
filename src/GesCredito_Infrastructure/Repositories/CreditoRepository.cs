using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using GesCredito.Domain.Enums;
using GesCredito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesCredito.Infrastructure.Repositories;

public class CreditoRepository(GesCreditoDbContext db) : ICreditoRepository
{
    public Task<Credito?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Creditos.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<(IReadOnlyList<Credito> Items, int Total)> BuscarAsync(
        int? clienteId, EstadoCredito? estado, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<Credito> q = db.Creditos.AsNoTracking().Include(c => c.Cliente);

        if (clienteId.HasValue) q = q.Where(c => c.ClienteId == clienteId.Value);
        if (estado.HasValue) q = q.Where(c => c.Estado == estado.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(c => c.FechaSolicitud).ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Credito credito, CancellationToken ct = default) =>
        await db.Creditos.AddAsync(credito, ct);

    public void Remove(Credito credito) => db.Creditos.Remove(credito);
}
