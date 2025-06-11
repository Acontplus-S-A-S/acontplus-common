using Common.Core.Abstractions;

namespace Common.TestApi.Services;

public interface IAtsService
{
    Task<ApiResponse> CheckValidationAsync(Dictionary<string, object> parameters);
    Task<DataSet> GetAsync(Dictionary<string, object> parameters);
}
public class AtsService(IAdoRepository adoRepository) : IAtsService
{
    private readonly string ModuleName = "FactElect.Ats_";

    public async Task<ApiResponse> CheckValidationAsync(Dictionary<string, object> parameters)
    {
        return await adoRepository.SpExecuteAsync<ApiResponse>($"{ModuleName}CheckValidation", parameters);
    }

    public async Task<DataSet> GetAsync(Dictionary<string, object> parameters)
    {
        return await adoRepository.GetDataSetAsync($"{ModuleName}Get", parameters, true, false);
    }
}

