using MediatR;
using OrderService.Domain;

namespace OrderService.Application;

public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler : INotificationHandler<BuyerAndPaymentMethodVerifiedDomainEvent>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task Handle(BuyerAndPaymentMethodVerifiedDomainEvent buyerAndPaymentMethodVerifiedEvent, CancellationToken cancellationToken)
    {
        var orderToUptade = await _orderRepository.GetById(buyerAndPaymentMethodVerifiedEvent.OrderId);
        orderToUptade.SetBuyerId(buyerAndPaymentMethodVerifiedEvent.Buyer.Id);
        orderToUptade.SetPaymentMethodId(buyerAndPaymentMethodVerifiedEvent.PaymentMethod.Id);
    }
}
