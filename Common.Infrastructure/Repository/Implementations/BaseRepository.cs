using System.Linq.Expressions;
using System.Reflection;
using Common.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore.Query;

namespace Common.Infrastructure.Repository.Implementations;

public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<BaseRepository<T>> _logger;

    public BaseRepository(DbContext context, ILogger<BaseRepository<T>> logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    #region Query Methods

    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetByIdAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(id);
            return await _dbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting entity by ID: {Id}", id);
            throw new RepositoryException($"Error getting entity by ID {id}", ex);
        }
    }

    public virtual async Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetFirstOrDefaultAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);
            var query = BuildQuery(includeProperties);
            return await query.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetFirstOrDefaultAsync");
            throw new RepositoryException("Error retrieving entity", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetAllAsync)}");
        try
        {
            var query = BuildQuery(includeProperties);
            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetAllAsync");
            throw new RepositoryException("Error retrieving all entities", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(FindAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);
            var query = BuildQuery(includeProperties);
            return await query.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in FindAsync");
            throw new RepositoryException("Error finding entities", ex);
        }
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(pagination, null, null, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        Expression<Func<T, bool>>? predicate = null,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetPagedAsync)}");
        try
        {
            ValidatePagination(pagination);

            var query = BuildQuery(includeProperties, false);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(pagination.SortBy))
            {
                query = ApplySorting(query, pagination.SortBy, pagination.SortDirection);
            }

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<T>(items, pagination.PageIndex, pagination.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetPagedAsync");
            throw new RepositoryException("Error retrieving paged results", ex);
        }
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(ExistsAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return await _dbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ExistsAsync");
            throw new RepositoryException("Error checking entity existence", ex);
        }
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(CountAsync)}");
        try
        {
            return predicate == null
                ? await _dbSet.CountAsync(cancellationToken).ConfigureAwait(false)
                : await _dbSet.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CountAsync");
            throw new RepositoryException("Error counting entities", ex);
        }
    }

    #endregion

    #region Persistence Methods

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(AddAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entity);
            var entry = await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding entity");
            throw new RepositoryException("Error adding entity", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(AddRangeAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return [];

            await _dbSet.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entityList;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding entity range");
            throw new RepositoryException("Error adding entity range", ex);
        }
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(UpdateAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating entity");
            throw new RepositoryException("Error updating entity", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(UpdateRangeAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return [];

            foreach (var entity in entityList)
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                entry.State = EntityState.Modified;
            }

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entityList;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating entity range");
            throw new RepositoryException("Error updating entity range", ex);
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(DeleteAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting entity");
            throw new RepositoryException("Error deleting entity", ex);
        }
    }

    public virtual async Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(DeleteByIdAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(id);

            var entity = await _dbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting entity by ID: {Id}", id);
            throw new RepositoryException($"Error deleting entity by ID {id}", ex);
        }
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(DeleteRangeAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return;

            _dbSet.RemoveRange(entityList);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting entity range");
            throw new RepositoryException("Error deleting entity range", ex);
        }
    }

    public virtual async Task DeleteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(DeleteAsync)}_Predicate");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (entities.Count > 0)
            {
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting entities by predicate");
            throw new RepositoryException("Error deleting entities by predicate", ex);
        }
    }

    public virtual async Task<int> BulkDeleteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(BulkDeleteAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return await _dbSet
                .Where(predicate)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in BulkDeleteAsync");
            throw new RepositoryException("Error performing bulk delete", ex);
        }
    }

    public virtual async Task<int> BulkUpdateAsync<TProperty>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TProperty>> propertyExpression,
        TProperty newValue,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(BulkUpdateAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(propertyExpression);

            return await _dbSet
                .Where(predicate)
                .ExecuteUpdateAsync(setters => setters.SetProperty(propertyExpression, newValue), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in BulkUpdateAsync");
            throw new RepositoryException("Error performing bulk update", ex);
        }
    }

    #endregion

    #region ExecuteUpdate/ExecuteDelete Methods (EF Core 7+)

    public virtual async Task<int> ExecuteUpdateAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(ExecuteUpdateAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(setPropertyCalls);

            return await _dbSet
                .Where(predicate)
                .ExecuteUpdateAsync(setPropertyCalls, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ExecuteUpdateAsync");
            throw new RepositoryException("Error performing bulk update", ex);
        }
    }

    public virtual async Task<int> ExecuteDeleteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(ExecuteDeleteAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return await _dbSet
                .Where(predicate)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ExecuteDeleteAsync");
            throw new RepositoryException("Error performing bulk delete", ex);
        }
    }

    #endregion

    #region Specification Pattern

    public virtual async Task<IEnumerable<T>> FindWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(FindWithSpecificationAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(specification);
            var query = BuildSpecificationQuery(specification);
            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in FindWithSpecificationAsync");
            throw new RepositoryException("Error retrieving entities with specification", ex);
        }
    }

    public virtual async Task<T?> GetFirstOrDefaultWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetFirstOrDefaultWithSpecificationAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(specification);
            var query = BuildSpecificationQuery(specification);
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetFirstOrDefaultWithSpecificationAsync");
            throw new RepositoryException("Error retrieving entity with specification", ex);
        }
    }

    public virtual async Task<PagedResult<T>> GetPagedWithSpecificationAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{nameof(GetPagedWithSpecificationAsync)}");
        try
        {
            ArgumentNullException.ThrowIfNull(specification);
            var query = BuildSpecificationQuery(specification);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var items = await query
                .Skip((specification.Pagination.PageIndex - 1) * specification.Pagination.PageSize)
                .Take(specification.Pagination.PageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<T>(
                items,
                specification.Pagination.PageIndex,
                specification.Pagination.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetPagedWithSpecificationAsync");
            throw new RepositoryException("Error retrieving paged results with specification", ex);
        }
    }

    #endregion

    #region Helper Methods

    protected virtual IQueryable<T> BuildQuery(
        string[]? includeProperties = null,
        bool tracking = true)
    {
        var query = tracking ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();

        if (includeProperties is { Length: > 0 })
        {
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
        }

        return query;
    }

    protected virtual IQueryable<T> BuildSpecificationQuery(ISpecification<T> specification)
    {
        var query = specification.IsTracking ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();

        // Apply criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderByExpressions is { Count: > 0 })
        {
            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var orderExpression in specification.OrderByExpressions)
            {
                orderedQuery = orderExpression.IsDescending
                    ? orderedQuery?.ThenByDescending(orderExpression.Expression)
                      ?? query.OrderByDescending(orderExpression.Expression)
                    : orderedQuery?.ThenBy(orderExpression.Expression)
                      ?? query.OrderBy(orderExpression.Expression);
            }

            if (orderedQuery is not null)
            {
                query = orderedQuery;
            }
        }

        return query;
    }

    protected virtual void ValidatePagination(PaginationDto pagination)
    {
        ArgumentNullException.ThrowIfNull(pagination);

        if (pagination.PageIndex < 1)
            throw new ArgumentException("Page index must be greater than 0", nameof(pagination));

        if (pagination.PageSize < 1 || pagination.PageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000", nameof(pagination));
    }

    protected virtual IQueryable<T> ApplySorting(
        IQueryable<T> query,
        string sortBy,
        string? sortDirection = null)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var property = typeof(T).GetProperty(sortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    #endregion

    #region IDisposable

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
