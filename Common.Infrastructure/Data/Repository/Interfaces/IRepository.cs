using System.Linq.Expressions;

namespace Common.Infrastructure.Data.Repository.Interfaces;

/// <summary>
/// Generic repository interface that defines common operations for data access
/// </summary>
/// <typeparam name="T">Entity type that will be managed by the repository</typeparam>
public interface IRepository<T> where T : class
{
    #region Query Methods

    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if exists, null otherwise</returns>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specified condition
    /// </summary>
    /// <param name="predicate">Condition expression</param>
    /// <param name="includeProperties">Related properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The first entity that matches the condition</returns>
    Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="includeProperties">Related properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync(
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities that match the specified condition
    /// </summary>
    /// <param name="predicate">Condition expression</param>
    /// <param name="includeProperties">Related properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities that match the condition</returns>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities in a paged manner
    /// </summary>
    /// <param name="pagination">Object with pagination information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result</returns>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities in a paged manner applying a filter
    /// </summary>
    /// <param name="pagination">Object with pagination information</param>
    /// <param name="predicate">Condition expression</param>
    /// <param name="includeProperties">Related properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result</returns>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified condition
    /// </summary>
    /// <param name="predicate">Condition expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if at least one entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the specified condition
    /// </summary>
    /// <param name="predicate">Condition expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities</returns>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistence Methods

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asynchronous task</returns>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a range of entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asynchronous task</returns>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>Asynchronous task</returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Updates a range of entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <returns>Asynchronous task</returns>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <returns>Asynchronous task</returns>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>Asynchronous task</returns>
    Task DeleteByIdAsync(object id);

    /// <summary>
    /// Deletes a range of entities
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    /// <returns>Asynchronous task</returns>
    Task DeleteRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes entities that match the specified condition
    /// </summary>
    /// <param name="predicate">Condition expression</param>
    /// <returns>Asynchronous task</returns>
    Task DeleteAsync(Expression<Func<T, bool>> predicate);

    #endregion
}
