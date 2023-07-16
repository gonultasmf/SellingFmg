using EventBus.Base.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BasketService.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;
    private readonly IIdentityService _identityService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketRepository basketRepository, 
        IIdentityService identityService,  
        ILogger<BasketController> logger,
        IEventBus eventBus)
    {
        _basketRepository = basketRepository;
        _identityService = identityService;
        _eventBus = eventBus;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Basket Service is Up and Running");
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetBasketByIdAsync(string id)
    {
        var basket = await _basketRepository.GetBasketAsync(id);

        return Ok(basket ?? new());
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateBasketAsync([FromBody]  CustomerBasket basket)
    {
        return Ok(await _basketRepository.UpdateBasketAsync(basket));
    }

    [HttpPost]
    [Route("additem")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> AddItemToBasket([FromBody] BasketItem basketItem)
    {
        var userId = _identityService.GetUserName();
        var basket = await _basketRepository.GetBasketAsync(userId);

        if(basket is null)
        {
             basket = new CustomerBasket(userId);
        }

        basket.Items.Add(basketItem);
        await _basketRepository.UpdateBasketAsync(basket);

        return Ok();
    }

    [HttpPost]
    [Route("checkout")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout)
    {
        var userId = basketCheckout.Buyer;
        var basket = await _basketRepository.GetBasketAsync(userId);

        if (basket is null) return BadRequest();

        var userName = _identityService.GetUserName();
        var eventMessage = new OrderCreatedIntegrationEvent(userId, userName, basketCheckout.City, basketCheckout.Country,
            basketCheckout.Street, basketCheckout.ZipCode, basketCheckout.State, basketCheckout.CardNumber, basketCheckout.CardHolderName,
            basketCheckout.CardExpiration, basketCheckout.CardSecurityNumber, basketCheckout.CardTypeId, basketCheckout.Buyer, basket);

        try
        {
            _eventBus.Publish(eventMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {BasketService.Api}", eventMessage.Id);

            throw;
        }

        return Accepted();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task DeleteBasketByIdAsync(string id)
    {
        await _basketRepository.DeleteBasketAsync(id);
    }
}
