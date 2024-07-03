namespace Common.Infrastructure.Repository;

public interface IAdoSqlServer
{
    public Task<List<T>> DynamicListAsync<T>(string spname, Dictionary<string, object> parameters = null,
        string connectionStringName = null);

    public Task<DataSet> GetDataSetAsync(string spname, Dictionary<string, object> parameters = null,
        bool timeout = true, string connectionStringName = null, bool withTableNames = true);

    public Task<DataTable> GetDataTableAsync(string spname, Dictionary<string, object> parameters = null,
        string connectionStringName = null);

    public Task<int> OnlyExecuteAsync(string query, Dictionary<string, object> parameters = null,
        bool useStoredProcedure = true, bool timeout = true, string connectionStringName = null);

    public Task<T> SpExecuteAsync<T>(string spname, Dictionary<string, object> parameters = null, bool timeout = true,
        string connectionStringName = null) where T : class, new();
}
