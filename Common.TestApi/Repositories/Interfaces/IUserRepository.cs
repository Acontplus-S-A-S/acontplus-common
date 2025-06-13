namespace Common.TestApi.Repositories.Interfaces;

public interface IUserRepository : IRepository<Usuario>
{
    Task<PagedResult<Usuario>> GetPaginatedUsersAsync(PaginationDto pagination);
}
