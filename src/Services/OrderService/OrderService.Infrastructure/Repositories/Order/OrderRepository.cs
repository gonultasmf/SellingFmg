using OrderService.Application;
using OrderService.Domain;
using System.Linq.Expressions;

namespace OrderService.Infrastructure;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly OrderDbContext _context;
    public OrderRepository(OrderDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<Order> GetById(Guid id, params Expression<Func<Order, object>>[] includes)
    {
        var entity = await base.GetById(id, includes);

        if (entity is null)
        {
            entity = _context.Orders.Local.FirstOrDefault(o => o.Id == id);
        }

        return entity;
    }
}
