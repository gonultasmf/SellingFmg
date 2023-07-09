using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService;
using RabbitMQ.Client;

internal class Program
{
    private static void Main(string[] args)
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);

        var sp = services.BuildServiceProvider();

        var eventBus = sp.GetService<IEventBus>();

        eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
        eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();


        Console.WriteLine("Application is running.....");
        Console.ReadLine();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(configure => configure.AddConsole());

        services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();
        services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();

        services.AddSingleton<IEventBus>(sp =>
        {
            EventBusConfig config = new()
            {
                ConnectionRetyCount = 5,
                EventNameSuffix = "IntegrationEvent",
                Connection = new ConnectionFactory(),
                EventBusType = EventBusType.RabbitMQ,
                SubscriberClientAppName = "NotificationService"
            };

            return EventBusFactory.Create(config, sp);
        });

    }
}