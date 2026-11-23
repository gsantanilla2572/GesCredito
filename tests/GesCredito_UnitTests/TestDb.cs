using GesCredito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesCredito.UnitTests;

internal static class TestDb
{
    /// <summary>Contexto EF Core InMemory aislado por prueba.</summary>
    public static GesCreditoDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GesCreditoDbContext>()
            .UseInMemoryDatabase($"gescredito-{Guid.NewGuid()}")
            .Options;
        return new GesCreditoDbContext(options);
    }
}
