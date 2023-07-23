using OrderService.Domain;

namespace OrderService.Application;

public interface IOrderRepository : IGenericRepository<Order>
{
}
