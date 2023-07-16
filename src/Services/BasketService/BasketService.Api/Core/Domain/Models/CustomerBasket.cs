namespace BasketService.Api;

public class CustomerBasket
{
    public string BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = new List<BasketItem>();


    public CustomerBasket()
    {
        
    }

    public CustomerBasket(string ıd)
    {
        BuyerId = ıd;
    }
}
