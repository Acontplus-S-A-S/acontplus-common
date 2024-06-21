namespace Common.Infrastructure.Repository;

public interface IAdoRepository
{
    public Task<List<T>> DynamicListAsync<T>(string spName, Dictionary<string, object> parameters);
    public Task<DataSet> GetDataSetAsync(string spName, Dictionary<string, object> parameters);
    public Task<DataTable> GetDataTableAsync(string spName, Dictionary<string, object> parameters);

    public Task<int> OnlyExecuteAsync(string query, Dictionary<string, object> parameters,
        bool useStoredProcedure = true, bool timeout = true);

    public Task<T> SpExecuteAsync<T>(string spName, Dictionary<string, object> parameters, bool timeout = true)
        where T : class, new();
}
