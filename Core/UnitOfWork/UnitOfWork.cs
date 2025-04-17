using System.Collections;
using ECPLibrary.Core.Repository;
using Microsoft.EntityFrameworkCore;
using static System.GC;

namespace ECPLibrary.Core.UnitOfWork;

public class UnitOfWork<TContext>(IServiceProvider provider) : IUnitOfWork<TContext>
    where TContext : DbContext
{
    private Hashtable? _repositories;
    private bool _disposed;

    public TContext Context { get; } = provider.GetRequiredService<TContext>();

    public IRepository<T> Repository<T>() where T : class
    {
        _repositories ??= new Hashtable();

        var type = typeof(T).Name;

        if (_repositories.ContainsKey(type)) 
            return (IRepository<T>)_repositories[type]!;
            
        var repositoryType = typeof(Repository<T>);

        var repositoryInstance = Activator.CreateInstance(repositoryType, Context);

        _repositories.Add(type, repositoryInstance);

        return (IRepository<T>)_repositories[type]!;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken) 
        => await Context.SaveChangesAsync(cancellationToken);

    public Task Rollback()
    {
        Context.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(entry => entry.Reload());
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        Dispose(true);
        SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing) Context.Dispose();
        _disposed = true;
    }
}