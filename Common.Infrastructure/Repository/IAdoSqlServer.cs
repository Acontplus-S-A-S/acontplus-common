namespace Common.Infrastructure.Repository;

public interface IAdoSqlServer
{
    public Task<List<T>> DynamicListAsync<T>(string spname, Dictionary<string, object> parameters,
        string connectionStringName = null);

    public Task<DataSet> GetDataSetAsync(string spname, Dictionary<string, object> parameters, bool withTableNames = true,
        bool timeout = true, string connectionStringName = null);

    public Task<DataTable> GetDataTableAsync(string spname, Dictionary<string, object> parameters,
        string connectionStringName = null);

    public Task<int> OnlyExecuteAsync(string query, Dictionary<string, object> parameters,
        bool useStoredProcedure = true, bool timeout = true, string connectionStringName = null);

    public Task<T> SpExecuteAsync<T>(string spname, Dictionary<string, object> parameters, bool timeout = true,
        string connectionStringName = null) where T : class, new();
}
