namespace Common.Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task<PagedResult<T>> GetPagedAsync(PaginationDto pagination);
}
