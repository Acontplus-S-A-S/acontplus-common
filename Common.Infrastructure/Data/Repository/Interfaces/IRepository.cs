namespace Common.Infrastructure.Data.Repository.Interfaces;

public interface IRepository<T> where T : class
{
    Task<PagedResult<T>> GetPagedAsync(PaginationDto pagination);
}
