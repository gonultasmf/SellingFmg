using EventBus.Base.Abstraction;
using EventBus.Base.Events;

namespace PaymentService.Api;

public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    private readonly IConfiguration configuration;
    private readonly IEventBus eventBus;
    private readonly ILogger<OrderStartedIntegrationEvent> logger;

    public OrderStartedIntegrationEventHandler(IConfiguration configuration, 
        IEventBus eventBus, 
        ILogger<OrderStartedIntegrationEvent> logger)
    {
        this.configuration = configuration;
        this.eventBus = eventBus;
        this.logger = logger;
    }

    public Task Handle(OrderStartedIntegrationEvent @event)
    {
        // ödeme işlemi yapılmış gibi devam(Banka sistemine bağlı değiliz :/ )
        string keyword = "PaymentSuccess";
        bool paymentSuccessFlag = configuration.GetValue<bool>(keyword);

        IntegrationEvent paymentEvent = paymentSuccessFlag
            ? new OrderPaymentSuccessIntegrationEvent(@event.OrderId)
            : new OrderPaymentFailedIntegrationEvent(@event.OrderId, "This is error message.");

        logger.LogInformation($"OrderCreatedIntegrationEventHandler in PaymentService is fired with PaymentSuccess: {paymentSuccessFlag}, orderId: {@event.OrderId}");

        eventBus.Publish(paymentEvent);

        return Task.CompletedTask;
    }
}
