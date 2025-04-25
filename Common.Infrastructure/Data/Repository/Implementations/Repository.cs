namespace Common.Infrastructure.Data.Repository.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<PagedResult<T>> GetPagedAsync(PaginationDto pagination)
    {
        IQueryable<T> query = _dbSet;

        // Apply filtering here if needed with pagination parameters

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting (if SortBy parameter exists in pagination)
        // This is just a placeholder, you'll need to implement dynamic sorting
        // based on your entity properties

        // Apply pagination
        var items = await query
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }
}
