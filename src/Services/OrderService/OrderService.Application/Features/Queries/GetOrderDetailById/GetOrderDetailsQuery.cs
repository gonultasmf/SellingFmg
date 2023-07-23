using MediatR;

namespace OrderService.Application;

public class GetOrderDetailsQuery : IRequest<OrderDetailViewModel>
{
    public Guid OrderId { get; set; }

    public GetOrderDetailsQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
