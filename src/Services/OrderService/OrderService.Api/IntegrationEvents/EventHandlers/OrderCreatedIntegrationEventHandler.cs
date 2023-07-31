using EventBus.Base.Abstraction;
using MediatR;
using OrderService.Application;

namespace OrderService.Api;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IMediator mediator;
    private readonly ILogger<OrderCreatedIntegrationEventHandler> logger;

    public OrderCreatedIntegrationEventHandler(IMediator mediator, ILogger<OrderCreatedIntegrationEventHandler> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", 
            @event.Id, typeof(OrderCreatedIntegrationEventHandler).Namespace, @event);

        var createOrderCommand = new CreateOrderCommand(@event.Basket.Items, @event.UserName,
            @event.City, @event.Street, @event.State, @event.Country, @event.ZipCode, @event.CardNumber,
            @event.CardHolderName, @event.CardExpiration, @event.CardSecurityNumber, @event.CardTypeId);

        await mediator.Send(createOrderCommand);
    }
}
