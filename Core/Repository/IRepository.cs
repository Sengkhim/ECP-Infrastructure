using System.Linq.Expressions;

namespace ECPLibrary.Core.Repository;

/// <summary>
/// Represents a generic repository interface for performing CRUD operations on entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the queryable collection of all entities.
    /// </summary>
    IQueryable<TEntity> Entities { get; }

    /// <summary>
    /// Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<TEntity?> GetByIdAsync(string id);

    /// <summary>
    /// Retrieves all entities from the repository asynchronously.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Retrieves a paginated list of entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An IQueryable containing the requested page of entities.</returns>
    Task<IQueryable<TEntity>> GetPagedResponseAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Adds a new entity to the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity in the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity from the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Finds entities that match the given predicate asynchronously.
    /// </summary>
    /// <param name="expression">The predicate to filter entities.</param>
    /// <returns>An IQueryable containing the matching entities.</returns>
    Task<IQueryable<TEntity>?> FindAsync(Expression<Func<TEntity, bool>> expression);
}
