using OrderService.Application;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public class BuyerRepository : GenericRepository<Buyer>, IBuyerRepository
{
    public BuyerRepository(OrderDbContext context) : base(context)
    {
    }
}
