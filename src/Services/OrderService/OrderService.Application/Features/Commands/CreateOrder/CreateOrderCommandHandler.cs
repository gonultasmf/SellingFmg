using EventBus.Base.Abstraction;
using MediatR;
using OrderService.Domain;

namespace OrderService.Application;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<bool> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var addr = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);

        Order dbOrder = new(request.UserName, addr, request.CardTypeId, request.CardNumber, request.CardSecurityNumber, request.CardHolderName,
            request.CardExpiration, null);

        foreach (var x in request.OrderItems)
        {
            dbOrder.AddOrderItem(x.ProductId, x.ProductName, x.UnitPrice, x.PictureUrl, x.Units);
        }

        await _orderRepository.AddAsync(dbOrder);
        await _orderRepository.UnitOfWork.SaveChangesAsync();

        var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(request.UserName);
        _eventBus.Publish(orderStartedIntegrationEvent);

        return true;
    }
}
