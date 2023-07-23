namespace OrderService.Domain;

public class CustomerBasket
{
    public string BuyerId { get; set; }
    public List<BasketItem> Items { get; set; }


    public CustomerBasket(string buyerId)
    {
        BuyerId = buyerId;
        Items = new List<BasketItem>();
    }
}
