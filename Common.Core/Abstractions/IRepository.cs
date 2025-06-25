using System.Linq.Expressions;
using Common.Core.Base;
using Common.Core.DTOs;

namespace Common.Core.Abstractions;

/// <summary>
/// Defines generic data access operations for entities.
/// </summary>
/// <typeparam name="T">The entity type, must derive from BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    #region Query Methods

    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching a predicate, with optional includes.
    /// </summary>
    Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities, with optional includes.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate, with optional includes.
    /// </summary>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged result of all entities.
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged result of filtered entities, with optional includes.
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistence Methods

    /// <summary>
    /// Adds a new entity. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity for update. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks multiple entities for update. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity for deletion. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity for deletion by ID. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks multiple entities for deletion. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks entities matching a predicate for deletion. Persisted by UnitOfWork.SaveChangesAsync.
    /// </summary>
    Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk delete directly in the database.
    /// </summary>
    Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk update for a single property directly in the database.
    /// </summary>
    Task<int> BulkUpdateAsync<TProperty>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TProperty>> propertyExpression,
        TProperty newValue,
        CancellationToken cancellationToken = default);

    #endregion

    #region Specification Pattern

    /// <summary>
    /// Finds entities using a specification.
    /// </summary>
    Task<IEnumerable<T>> FindWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity using a specification.
    /// </summary>
    Task<T?> GetFirstOrDefaultWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged result of entities using a specification.
    /// </summary>
    Task<PagedResult<T>> GetPagedWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities using a specification.
    /// </summary>
    Task<int> CountWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    #endregion
}
