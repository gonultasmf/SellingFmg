using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;

namespace NotificationService;

internal class OrderPaymentSuccessIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
{
    private readonly ILogger<OrderPaymentSuccessIntegrationEvent> logger;

    public OrderPaymentSuccessIntegrationEventHandler(ILogger<OrderPaymentSuccessIntegrationEvent> logger)
    {
        this.logger = logger;
    }

    public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
    {
        // bu kısımda sms gönderimi yapılabilir veya mail gönderimi vs vs.
        logger.LogInformation($"Order Payment success with OrderId: {@event.OrderId}");

        return Task.CompletedTask;
    }
}
