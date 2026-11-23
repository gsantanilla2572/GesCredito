using System.Text.Json;
using System.Text.Json.Serialization;
using GesCredito.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GesCredito.IntegrationTests;

/// <summary>Levanta la API real en memoria sustituyendo SQL Server por EF Core InMemory.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"gescredito-it-{Guid.NewGuid()}";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // Quita toda la configuración del DbContext registrada con SQL Server.
            var obsoletos = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GenericTypeArguments.Contains(typeof(GesCreditoDbContext)))
                .ToList();
            foreach (var d in obsoletos) services.Remove(d);

            services.AddDbContext<GesCreditoDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}
