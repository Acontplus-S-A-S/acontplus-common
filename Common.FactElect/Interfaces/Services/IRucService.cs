namespace Common.FactElect.Interfaces.Services;

public interface IRucService
{
    Task<RucModel> GetRucSriAsync(string ruc);
}
