using GesCredito.Domain.Contracts;
using GesCredito.Infrastructure.Data;
using GesCredito.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GesCredito.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("GesCredito")
                 ?? throw new InvalidOperationException("Falta la cadena de conexión 'GesCredito'.");

        services.AddDbContext<GesCreditoDbContext>(o =>
            o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(3)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GesCreditoDbContext>());
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICreditoRepository, CreditoRepository>();
        return services;
    }
}
