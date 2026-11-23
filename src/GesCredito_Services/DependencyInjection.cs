using GesCredito.Services.Implementations;
using GesCredito.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GesCredito.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ICreditoService, CreditoService>();
        return services;
    }
}
