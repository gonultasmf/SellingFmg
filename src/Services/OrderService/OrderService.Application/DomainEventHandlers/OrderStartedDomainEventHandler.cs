using MediatR;
using OrderService.Domain;

namespace OrderService.Application;

public class OrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly IBuyerRepository _buyerRepository;

    public OrderStartedDomainEventHandler(IBuyerRepository buyerRepository)
    {
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
    }

    public async Task Handle(OrderStartedDomainEvent orderStartedEvent, CancellationToken cancellationToken)
    {
       var cardTypeId = (orderStartedEvent.CardTypeId != 0) ? orderStartedEvent.CardTypeId : 1;

        var buyer = await _buyerRepository.GetSingleAsync(x => x.Name == orderStartedEvent.UserName, x => x.PaymentMethods);

        bool buyerOriginallyExisted = buyer != null;

        if (buyerOriginallyExisted)
        {
            buyer = new Buyer(orderStartedEvent.UserName);
        }

        buyer.VerifyOrAddPaymentMethod(cardTypeId, $"Payment Method on {DateTime.UtcNow}", orderStartedEvent.CardNumber,
            orderStartedEvent.CardSecurityNumber, orderStartedEvent.CardHolderName, orderStartedEvent.CardExpiration,
            orderStartedEvent.Order.Id);

        var buyerUpdated = buyerOriginallyExisted ?
            _buyerRepository.Update(buyer) :
            await _buyerRepository.AddAsync(buyer);
    }
}
