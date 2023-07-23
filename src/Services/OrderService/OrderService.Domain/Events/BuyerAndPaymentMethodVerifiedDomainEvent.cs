using MediatR;
using System;

namespace OrderService.Domain;

public class BuyerAndPaymentMethodVerifiedDomainEvent : INotification
{
    public Buyer Buyer { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid OrderId { get; set; }


    public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod paymentMethod, Guid orderId)
    {
        Buyer = buyer;
        PaymentMethod = paymentMethod;
        OrderId = orderId;
    }

}
