using MediatR;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEventAsync(this IMediator mediator, OrderDbContext context)
    {
        var domainEntities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        var domainEvents = domainEntities
            .Select(x => x.Entity.DomainEvents)
            .ToList();

        foreach (var item in domainEntities)
        {
            item.Entity.ClearDomainEvents();
        }

        foreach (var item in domainEvents)
        {
            await mediator.Publish(item);
        }
    }
}
