using EventBus.Base.Abstraction;

namespace BasketService.Api;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IBasketRepository _basketRepository;
    private readonly ILogger<OrderCreatedIntegrationEvent> _logger;

    public OrderCreatedIntegrationEventHandler(IBasketRepository basketRepository, ILogger<OrderCreatedIntegrationEvent> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at BasketService.Api - ({@IntegrationEvent})", @event.Id);

        await _basketRepository.DeleteBasketAsync(@event.UserId);
    }
}
