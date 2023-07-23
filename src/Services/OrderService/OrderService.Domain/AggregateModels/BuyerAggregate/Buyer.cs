namespace OrderService.Domain;

public class Buyer : BaseEntity, IAggregateRoot
{
    public string Name { get; set; }
    private List<PaymentMethod> _paymentMethods;
    public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods;

    protected Buyer()
    {
        _paymentMethods = new List<PaymentMethod>();
    }

    public Buyer(string name):this()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public PaymentMethod VerifyOrAddPaymentMethod(int cardTypeId, string alias, string cardNumber,
        string securityNumber, string cardHolderName, DateTime expiration, Guid orderId)
    {
        var existingPayment = _paymentMethods.SingleOrDefault(x => x.IsEqualTo(cardTypeId, cardNumber, expiration));

        if (existingPayment is not null)
        {
            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));

            return existingPayment;
        }

        var payment = new PaymentMethod(alias, cardNumber, securityNumber, cardHolderName, expiration, cardTypeId);
        _paymentMethods.Add(payment);

        AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

        return payment;
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj) || 
            (obj is Buyer buyer &&
            Id.Equals(buyer.Id) &&
            Name == buyer.Name);
    }
}
