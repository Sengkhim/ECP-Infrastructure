using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ECPLibrary.Services;

/// <summary>
/// Represents a contract for a database context that abstracts Entity Framework Core operations.
/// </summary>
public interface IEcpDatabase
{
    /// <summary>
    /// Provides access to the database facade, allowing execution of raw SQL queries and database-related operations.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Gets a DbSet instance for the specified entity type, allowing CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>A DbSet for the specified entity type.</returns>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Retrieves the Entity Framework tracking entry for the given entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="entity">The entity to track.</param>
    /// <returns>The EntityEntry object providing access to tracking information.</returns>
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Marks the specified entity for deletion.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="entity">The entity to be removed.</param>
    /// <returns>The EntityEntry for the removed entity.</returns>
    EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Attaches the given entity to the context, allowing it to be tracked without modifying its state.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="entity">The entity to attach.</param>
    /// <returns>The EntityEntry for the attached entity.</returns>
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Saves the changes made to the context.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChanges();

    /// <summary>
    /// Saves the changes made to the context asynchronously with support for cancellation.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
