using System.Diagnostics;
using System.Linq.Expressions;
using ECPLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Core.Repository;

public class Repository<TEntity>(IEcpDatabase context) : IRepository<TEntity>
    where TEntity : class
{
    public IQueryable<TEntity> Entities => context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(string id)
        => await context
            .Set<TEntity>()
            .FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await context
            .Set<TEntity>()
            .ToListAsync();

    public async Task<IQueryable<TEntity>> GetPagedResponseAsync(int pageNumber, int pageSize)
    {
        var query  = context.Set<TEntity>()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .AsQueryable();
        
        return await Task.FromResult(query);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Added;
        await context.Set<TEntity>().AddAsync(entity);
        return entity;
    }
    
    public Task UpdateAsync(TEntity entity)
    {
        context.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Deleted;
        context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(string id)
    {
        Debug.Assert(context != null, nameof(context) + " != null");
        var data = GetByIdAsync(id);
        context.Set<TEntity>().Remove(data.Result!);
        return Task.CompletedTask;
    }

    public async Task<IQueryable<TEntity>?> FindAsync(Expression<Func<TEntity, bool>> expression)
    {
        var result = context.Set<TEntity>()
            .AsQueryable()
            .Where(expression);
        
        return await Task.FromResult(result);
    }
}