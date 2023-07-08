using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace EventBus.UnitTest;

[TestClass]
public class EventBusTests
{
    private ServiceCollection services;

    public EventBusTests()
    {
        services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());
    }


    [TestMethod]
    public void Subscribe_Event_On_RabbitMQ_Test()
    {
        services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetRabbitMQConfig(), sp);
        });

        var sp = services.BuildServiceProvider();

        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
    }

    [TestMethod]
    public void Send_Message_To_RabbitMQ_Test()
    {
        services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetRabbitMQConfig(), sp);
        });

        var sp = services.BuildServiceProvider();

        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Publish(new OrderCreatedIntegrationEvent(2));
    }

    private EventBusConfig GetRabbitMQConfig()
    {
        return new()
        {
            ConnectionRetyCount = 5,
            SubscriberClientAppName = "EventBus.UnitTest",
            DefaultTopicName = "SellingFmgTopicName",
            EventBusType = EventBusType.RabbitMQ,
            EventNameSuffix = "IntegrationEvent",
            Connection = new ConnectionFactory()
        };
    }
}