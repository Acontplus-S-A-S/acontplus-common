namespace Common.Infrastructure.Repository.Interfaces;

public interface IAdoRepository
{
    public Task<List<T>> DynamicListAsync<T>(string spName, Dictionary<string, object> parameters = null);
    public Task<DataSet> GetDataSetAsync(string spName, Dictionary<string, object> parameters = null, bool withTableNames = true, bool timeout = true);
    public Task<DataTable> GetDataTableAsync(string spName, Dictionary<string, object> parameters = null);

    public Task<int> OnlyExecuteAsync(string query, Dictionary<string, object> parameters = null,
        bool useStoredProcedure = true, bool timeout = true);

    public Task<T> SpExecuteAsync<T>(string spName, Dictionary<string, object> parameters = null, bool timeout = true)
        where T : class, new();

    public Task<string> SpExecuteDeprecatedAsync(string spName, Dictionary<string, object> parameters,
    bool timeout = true);
}
