using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;

namespace NotificationService;

internal class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
{
    private readonly ILogger<OrderPaymentFailedIntegrationEvent> logger;

    public OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEvent> logger)
    {
        this.logger = logger;
    }

    public Task Handle(OrderPaymentFailedIntegrationEvent @event)
    {
        // bu kısımda sms gönderimi yapılabilir veya mail gönderimi vs vs.
        logger.LogInformation($"Order Payment failed with OrderId: {@event.OrderId}, ErrorMessage: {@event.ErrorMessage}");

        return Task.CompletedTask;
    }
}
