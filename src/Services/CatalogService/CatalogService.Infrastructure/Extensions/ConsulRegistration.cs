using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure;

public static class ConsulRegistration
{
    public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
        {
            var address = configuration["ConsulConfig:Address"];

            consulConfig.Address = new Uri(address);
        }));

        return services;
    }

    public static IApplicationBuilder RegistrationWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifeTime)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

        var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

        var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

        var feature = app.Properties["server.Features"] as FeatureCollection;
        var addresses = feature.Get<IServerAddressesFeature>();
        var address = addresses.Addresses.FirstOrDefault();

        var url = new Uri(address);
        var registration = new AgentServiceRegistration()
        {
            ID = $"CatalogService",
            Name = "CatalogService",
            Address = $"{url.Host}",
            Port = url.Port,
            Tags = new[] { "Catalog Service", "Catalog" }
        };

        logger.LogInformation("Registering with Consul");
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        lifeTime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering with Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
