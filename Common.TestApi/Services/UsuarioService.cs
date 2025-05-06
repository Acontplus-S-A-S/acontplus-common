using Common.Core.DTOs;
using Common.Infrastructure.Data.Repository.Interfaces;
using Common.TestApi.Data;
using Common.TestApi.DTOs;
using Common.TestApi.Entities;
using Common.TestApi.Repositories.Interfaces;

namespace Common.TestApi.Services
{
    public interface IUsuarioService
    {
        Task<ApiResponse> AddAsync(Usuario usuario);
        Task<int> CreateAsync();
        Task<PagedResult<UsuarioDto>> GetPaginatedUsersAsync(PaginationDto pagination);
        Task<ApiResponse> UpdateAsync(int id, Usuario usuario);
    }
    public class UsuarioService(TestContext context, IUserRepository userRepository, IAdoRepository adoRepository) : IUsuarioService
    {
        public async Task<ApiResponse> AddAsync(Usuario usuario)
        {
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            return ApiResponse.Success();
        }
        public async Task<int> CreateAsync()
        {
            return await adoRepository.ExecuteNonQueryAsync("INSERT INTO Test.WorkerTest(Content) VALUES ('Inserting')", useStoredProcedure: false);
        }

        public async Task<PagedResult<UsuarioDto>> GetPaginatedUsersAsync(PaginationDto paginationDto)
        {
            // Get paged data from repository
            var pagedUsers = await userRepository.GetPaginatedUsersAsync(paginationDto);

            // Map to DTOs
            var userDtos = pagedUsers.Items.Select(user => ObjectMapper.Map<Usuario, UsuarioDto>(user)).ToList();

            // Create new paged result with DTOs
            return new PagedResult<UsuarioDto>
            {
                Items = userDtos,
                PageIndex = pagedUsers.PageIndex,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount
            };
        }
       
 
        public async Task<ApiResponse> UpdateAsync(int id, Usuario usuario)
        {
            var userFound = await context.Usuarios.FindAsync(id);
            if (userFound == null)
            {
                return new ApiResponse { Code = "0", Message = "Error" };
            }

            context.Update(userFound);
            await context.SaveChangesAsync();
            //return new ApiResponse { Code = "1", Message = "Sucess", Payload = usuario };
            return ApiResponse.Success(payload: usuario);

        }
    }
}
