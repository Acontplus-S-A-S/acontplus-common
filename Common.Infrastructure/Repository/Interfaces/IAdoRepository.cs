namespace Common.Infrastructure.Repository.Interfaces;

public interface IAdoRepository
{
    public Task<List<T>> DynamicListAsync<T>(string spName, Dictionary<string, object> parameters = null,
        string connectionStringName = null, CancellationToken cancellationToken = default);

    public Task<DataSet> GetDataSetAsync(string spName, Dictionary<string, object> parameters = null,
        bool withTableNames = true, bool timeout = true, string connectionStringName = null,
        CancellationToken cancellationToken = default);

    public Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null,
        bool useStoredProcedure = true, bool timeout = true, string connectionStringName = null,
        CancellationToken cancellationToken = default);

    public Task<T> SpExecuteAsync<T>(string spName, Dictionary<string, object> parameters = null, bool timeout = true,
        string connectionStringName = null, CancellationToken cancellationToken = default)
        where T : class, new();

    //DEPRECATED
    public Task<DataTable> GetDataTableAsync(string spname, Dictionary<string, object> parameters = null,
    bool timeout = true,
    string connectionStringName = null, CancellationToken cancellationToken = default);


    public Task<string> SpExecuteDeprecatedAsync(string spName, Dictionary<string, object> parameters,
        bool timeout = true, string connectionStringName = null,
        CancellationToken cancellationToken = default);
}
