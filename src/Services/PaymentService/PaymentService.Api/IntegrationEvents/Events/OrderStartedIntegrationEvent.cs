using EventBus.Base.Events;

namespace PaymentService.Api;

public class OrderStartedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }

    public OrderStartedIntegrationEvent()
    {

    }

    public OrderStartedIntegrationEvent(int orderId)
    {
        OrderId = orderId;
    }
}
