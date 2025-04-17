using ECPLibrary.Core.Repository;

namespace ECPLibrary.Core.UnitOfWork;

/// <summary>
/// Represents the Unit of Work pattern for managing transactions and repositories.
/// </summary>
/// <typeparam name="TContext">The type of database context.</typeparam>
public interface IUnitOfWork<out TContext> : IDisposable
{
    /// <summary>
    /// Gets the database context associated with the unit of work.
    /// </summary>
    TContext Context { get; }

    /// <summary>
    /// Gets a repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <returns>An instance of IRepository for the specified entity type.</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Commits all changes made within the unit of work asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back any changes made within the unit of work.
    /// </summary>
    Task Rollback();
}
