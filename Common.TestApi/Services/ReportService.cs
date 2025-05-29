using Common.Core.Abstractions;

namespace Common.TestApi.Services;

public interface IReportService
{
    Task<DataSet> GetParamsAsync(Dictionary<string, object> parameters);
    Task<DataSet> GetDataAsync(string spname, Dictionary<string, object> parameters, bool withTableNames = false);
}
public class ReportService(IAdoRepository adoRepository) : IReportService
{
    public async Task<DataSet> GetParamsAsync(Dictionary<string, object> parameters)
    {
        return await adoRepository.GetDataSetAsync("Reporte.Report_Get", parameters);
    }

    public async Task<DataSet> GetDataAsync(string spname, Dictionary<string, object> parameters, bool withTableNames)
    {
        return await adoRepository.GetDataSetAsync(spname, parameters, withTableNames);
    }
}
