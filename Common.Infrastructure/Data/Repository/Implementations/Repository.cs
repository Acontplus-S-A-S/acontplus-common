using System.Linq.Expressions;

namespace Common.Infrastructure.Data.Repository.Implementations;

/// <summary>
/// Generic repository implementation that provides common data access operations
/// </summary>
/// <typeparam name="T">Entity type that will be managed by the repository</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructor that initializes the repository with a database context
    /// </summary>
    /// <param name="context">Database context</param>
    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    #region Query Methods Implementation

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include related properties if specified
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync(
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include related properties if specified
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include related properties if specified
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting if provided in pagination
        if (!string.IsNullOrEmpty(pagination.SortBy))
        {
            query = ApplySorting(query, pagination.SortBy, pagination.SortDirection);
        }

        // Apply pagination
        var items = await query
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
        };
    }

    /// <inheritdoc />
    public virtual async Task<PagedResult<T>> GetPagedAsync(
        PaginationDto pagination,
        Expression<Func<T, bool>> predicate,
        string[]? includeProperties = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include related properties if specified
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }

        // Apply filtering
        query = query.Where(predicate);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting if provided in pagination
        if (!string.IsNullOrEmpty(pagination.SortBy))
        {
            query = ApplySorting(query, pagination.SortBy, pagination.SortDirection);
        }

        // Apply pagination
        var items = await query
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
        };
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Persistence Methods Implementation

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteByIdAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = await FindAsync(predicate);
        await DeleteRangeAsync(entities);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Applies sorting to a query based on property name and direction
    /// </summary>
    /// <param name="query">Query to apply sorting to</param>
    /// <param name="sortBy">Property name to sort by</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <returns>Sorted query</returns>
    protected virtual IQueryable<T> ApplySorting(IQueryable<T> query, string sortBy, string? sortDirection)
    {
        // This is a basic implementation that assumes sortBy is a valid property name
        // In a production environment, you would want to add more validation and error handling
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    #endregion
}
