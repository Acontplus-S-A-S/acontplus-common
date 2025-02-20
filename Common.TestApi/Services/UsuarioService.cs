using Common.TestApi.Data;
using Common.TestApi.Entities;

namespace Common.TestApi.Services
{
    public interface IUsuarioService
    {
        Task<ApiResponse> AddAsync(Usuario usuario);
        Task<ApiResponse> UpdateAsync(int id, Usuario usuario);
    }
    public class UsuarioService(TestContext context) : IUsuarioService
    {
        public async Task<ApiResponse> AddAsync(Usuario usuario)
        {
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            return new ApiResponse { Code = "1", Message = " Success", Payload = usuario };
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
            return new ApiResponse { Code = "1", Message = "Sucess", Payload = usuario };

        }
    }
}
