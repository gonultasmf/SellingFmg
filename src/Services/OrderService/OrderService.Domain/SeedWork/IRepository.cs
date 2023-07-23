namespace OrderService.Domain;

public interface IRepository<T>
{
    IUnitOfWork UnitOfWork { get; }
}
