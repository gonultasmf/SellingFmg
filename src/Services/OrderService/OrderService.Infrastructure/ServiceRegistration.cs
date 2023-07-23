using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application;

namespace OrderService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SeelingFMG_Connection"));
        });

        services.AddScoped<IBuyerRepository, BuyerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();


        return services;
    }
}
