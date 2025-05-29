using System.Linq.Expressions;

namespace Common.Core.Abstractions;

/// <summary>
/// Generic repository interface defining common data access operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> : IDisposable where T : class
{
    #region Query Methods

    /// <summary>
    /// Gets an entity by its primary key
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The first matching entity or null</returns>
    Task<T> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string[] includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All entities</returns>
    Task<IEnumerable<T>> GetAllAsync(
        string[] includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities that match the predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching entities</returns>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        string[] includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged result of entities
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result</returns>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged result of entities that match the predicate
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result</returns>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        Expression<Func<T, bool>> predicate,
        string[] includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any match exists</returns>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities that match the predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(
        Expression<Func<T, bool>> predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistence Methods

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entities</returns>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entities</returns>
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its primary key
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes entities that match the predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk delete operation
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of deleted records</returns>
    Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk update operation
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="propertyExpression">Property to update</param>
    /// <param name="newValue">New value for the property</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of updated records</returns>
    Task<int> BulkUpdateAsync<TProperty>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TProperty>> propertyExpression,
        TProperty newValue,
        CancellationToken cancellationToken = default);

    #endregion
}
