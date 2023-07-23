using OrderService.Domain;
using System.Linq.Expressions;

namespace OrderService.Application;

public interface IGenericRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAll();
    Task<List<T>> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> func = null);
    Task<List<T>> Get(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
    Task<T> GetById(Guid id);
    Task<T> GetById(Guid id, params Expression<Func<T, object>>[] includes);
    Task<T> GetSingleAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    T Update(T entity);
}
