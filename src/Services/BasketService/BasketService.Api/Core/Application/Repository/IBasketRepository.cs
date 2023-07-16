namespace BasketService.Api;

public interface IBasketRepository
{
    Task<CustomerBasket> GetBasketAsync(string customerId);
    IEnumerable<string> GetUsers();
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync(string id);
}
