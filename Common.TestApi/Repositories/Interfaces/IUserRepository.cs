using Common.Core.Abstractions;
using Common.TestApi.Entities;

namespace Common.TestApi.Repositories.Interfaces;

public interface IUserRepository : IRepository<Usuario>
{
    Task<PagedResult<Usuario>> GetPaginatedUsersAsync(PaginationDto pagination);
}
