using EventBus.Base.Events;

namespace BasketService.Api;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public string UserId { get; }
    public string UserName { get; }
    public int OrderNumber { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Street { get; set; }
    public string ZipCode { get; set; }
    public string State { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime CardExpiration { get; set; }
    public string CardSecurityNumber { get; set; }
    public int CardTypeId { get; set; }
    public string Buyer { get; set; }
    public Guid RequestId { get; set; }
    public CustomerBasket CustomerBasket { get; }


    public OrderCreatedIntegrationEvent(string userId, string userName,
        string city, string country, string street, string zipCode, string state,
        string cardNumber, string cardHolderName, DateTime cardExpiration,
        string cardSecurityNumber, int cardTypeId, string buyer,
        CustomerBasket customerBasket)
    {
        UserId = userId;
        UserName = userName;
        City = city;
        Country = country;
        Street = street;
        ZipCode = zipCode;
        State = state;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
        Buyer = buyer;
        CustomerBasket = customerBasket;
    }
}
