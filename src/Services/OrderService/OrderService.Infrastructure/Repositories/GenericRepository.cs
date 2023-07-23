using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain;
using System.Linq.Expressions;

namespace OrderService.Infrastructure;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly OrderDbContext _context;

    public GenericRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); ;
    }

    public IUnitOfWork UnitOfWork { get; }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);

        return entity;
    }

    public virtual async Task<List<T>> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        foreach (var item in includes)
        {
            query = query.Include(item);
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.ToListAsync();
    }

    public virtual Task<List<T>> Get(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes)
    {
        return Get(filter, null, includes);
    }

    public virtual async Task<List<T>> GetAll()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public virtual async Task<T> GetById(Guid id)
    {
       return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<T> GetById(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        foreach (var item in includes)
        {
            query = query.Include(item);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<T> GetSingleAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        foreach (var item in includes)
        {
            query = query.Include(item);
        }

        return await query.Where(filter).SingleOrDefaultAsync();
    }

    public T Update(T entity)
    {
        _context.Set<T>().Update(entity);

        return entity;
    }
}
