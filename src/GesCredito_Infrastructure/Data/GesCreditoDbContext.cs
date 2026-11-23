using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GesCredito.Infrastructure.Data;

public class GesCreditoDbContext(DbContextOptions<GesCreditoDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Credito> Creditos => Set<Credito>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GesCreditoDbContext).Assembly);
    }
}
