using Common.Infrastructure.Repository.Interfaces;
using System.Data;

namespace Common.TestApi.Services;

public interface IEmailService
{
    public Task<DataTable> GetAsync(int quantity);
    public Task<int> UpdateAsync(int id, string estado, string msgError = null);
}

public sealed class NotificacionService(IAdoSqlServer repository) : IEmailService
{
    public async Task<DataTable> GetAsync(int cantidad)
    {
        return await repository.GetDataTableAsync("App.Notificacion_Serv_Get",
            new Dictionary<string, object> { { "cantidad", cantidad } });
    }

    public async Task<int> UpdateAsync(int id, string estado, string msgError)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "estado", estado },
            { "msgError", msgError ?? "" }
        };
        return await repository.OnlyExecuteAsync("App.Notificacion_Serv_Update", parameters);
    }
}
