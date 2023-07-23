using EventBus.Base.Events;

namespace OrderService.Application;

public class OrderStartedIntegrationEvent : IntegrationEvent
{
    public string UserName { get; set; }

    public OrderStartedIntegrationEvent(string userName)
    {
        UserName = userName;
    }
}
