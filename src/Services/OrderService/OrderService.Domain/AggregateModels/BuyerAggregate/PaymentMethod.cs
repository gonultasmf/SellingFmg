namespace OrderService.Domain;

public class PaymentMethod : BaseEntity
{
    public string Alias { get; set; }
    public string CardNumber { get; set; }
    public string SecurityNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime Expiration { get; set; }
    public int CardTypeId { get; set; }
    public CardType CardType { get; private set; }

    public PaymentMethod() { }

    public PaymentMethod(string alias, string cardNumber, string securityNumber, 
        string cardHolderName, DateTime expiration, int cardTypeId)
    {
        Alias = alias;
        CardNumber = !string.IsNullOrWhiteSpace(cardNumber) ? cardNumber : throw new OrderingDoaminException(nameof(cardNumber));
        SecurityNumber = !string.IsNullOrWhiteSpace(securityNumber) ? securityNumber : throw new OrderingDoaminException(nameof(securityNumber));
        CardHolderName = !string.IsNullOrWhiteSpace(cardHolderName) ? cardHolderName : throw new OrderingDoaminException(nameof(cardHolderName));
        if (expiration < DateTime.UtcNow) throw new OrderingDoaminException(nameof(expiration));
        Expiration = expiration;
        CardTypeId = cardTypeId;
    }

    public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration) =>
        CardTypeId == cardTypeId && CardNumber == cardNumber && Expiration == expiration;
}
