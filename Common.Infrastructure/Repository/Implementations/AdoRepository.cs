using Common.Infrastructure.Mapping;

namespace Common.Infrastructure.Repository.Implementations;

public class AdoRepository(
    IConfiguration configuration,
    ILogger<AdoRepository> logger)
    : IAdoRepository
{
    private readonly ConcurrentDictionary<string, string> _connectionStrings = new();

    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<SqlException>(ex => IsTransientException(ex))
        .Or<TimeoutException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    private string GetConnectionString(string name)
    {
        // Use "DefaultConnection" if name is null or empty
        var key = string.IsNullOrEmpty(name) ? "DefaultConnection" : name;

        return _connectionStrings.GetOrAdd(key, k =>
        {
            var connString = configuration.GetConnectionString(k);
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException($"Connection string '{k}' not found");
            }

            return connString;
        });
    }

    private static bool IsTransientException(SqlException ex)
    {
        var transientErrorNumbers = new[] { 4060, 40197, 40501, 40613, 49918, 49919, 49920, 4221 };
        return transientErrorNumbers.Contains(ex.Number);
    }

    private async Task<SqlConnection> CreateConnectionAsync(string connectionStringName,
        CancellationToken cancellationToken)
    {
        try
        {
            var connection = new SqlConnection(GetConnectionString(connectionStringName));
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating connection for {ConnectionName}", connectionStringName);
            throw;
        }
    }

    public async Task<List<T>> DynamicListAsync<T>(
        string spName,
        Dictionary<string, object> parameters,
        bool disableTimeout,
        string connectionStringName,
        CancellationToken cancellationToken)
    {
        parameters ??= new Dictionary<string, object>();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            await using var connection = await CreateConnectionAsync(connectionStringName, cancellationToken);
            await using var cmd = CreateCommand(connection, spName, parameters, CommandType.StoredProcedure, disableTimeout);

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                return await DbDataReaderMapper.ToListAsync<T>(reader, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing DynamicListAsync for {SpName}", spName);
                throw;
            }
        });
    }

    public async Task<DataSet> GetDataSetAsync(
        string spName,
        Dictionary<string, object> parameters,
        bool withTableNames,
        bool disableTimeout,
        string connectionStringName,
        CancellationToken cancellationToken,
        int tableNamesLength)
    {
        parameters ??= new Dictionary<string, object>();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            await using var connection = await CreateConnectionAsync(connectionStringName, cancellationToken);
            await using var cmd = CreateCommand(connection, spName, parameters, CommandType.StoredProcedure, disableTimeout);

            if (withTableNames)
            {
                const string outParam = "@tableNames";
                CommandParameterBuilder.AddOutputParameter(cmd, outParam, SqlDbType.VarChar, tableNamesLength);
            }

            var ds = new DataSet();
            try
            {
                using var adapter = new SqlDataAdapter(cmd);
                await Task.Run(() => adapter.Fill(ds), cancellationToken);

                if (withTableNames)
                {
                    await ProcessTableNames(cmd, ds);
                }

                return ds;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing GetDataSetAsync for {SpName}", spName);
                throw;
            }
        });
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string commandText,
        Dictionary<string, object> parameters,
        CommandType commandType,
        bool disableTimeout = false)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.CommandType = commandType;

        if (disableTimeout)
        {
            cmd.CommandTimeout = 0;
        }

        foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
        {
            CommandParameterBuilder.AddParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
        }

        return cmd;
    }

    private static async Task ProcessTableNames(SqlCommand cmd, DataSet ds)
    {
        var tableNames = cmd.Parameters["@tableNames"].Value?.ToString()?.Split(',');
        if (tableNames == null) return;

        await Task.Run(() =>
        {
            Parallel.ForEach(tableNames, (tableName, _, index) =>
            {
                if (!string.IsNullOrEmpty(tableName))
                {
                    ds.Tables[(int)index].TableName = tableName;
                }
            });
        });
    }

    public async Task<T> SpExecuteAsync<T>(
        string spName,
        Dictionary<string, object> parameters,
        bool disableTimeout,
        string connectionStringName,
        CancellationToken cancellationToken) where T : class, new()
    {
        parameters ??= new Dictionary<string, object>();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            await using var connection = await CreateConnectionAsync(connectionStringName, cancellationToken);
            await using var cmd = CreateCommand(connection, spName, parameters, CommandType.StoredProcedure, disableTimeout);

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                return await reader.ReadAsync(cancellationToken)
                    ? await MapToObject<T>(reader)
                    : new T();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing SpExecuteAsync for {SpName}", spName);
                throw;
            }
        });
    }

    private static async Task<T> MapToObject<T>(SqlDataReader reader) where T : class, new()
    {
        var result = new T();
        var properties = typeof(T).GetProperties();
        var schemaTable = await reader.GetSchemaTableAsync();

        foreach (var property in properties)
        {
            var columnName = schemaTable?.Rows.Cast<DataRow>()
                .Where(row => row["ColumnName"].ToString()!
                    .Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                .Select(row => row["ColumnName"].ToString())
                .FirstOrDefault();

            if (columnName == null) continue;

            var index = reader.GetOrdinal(columnName);
            if (index != -1 && !reader.IsDBNull(index))
            {
                property.SetValue(result, reader.GetValue(index));
            }
        }

        return result;
    }

    public async Task<int> ExecuteNonQueryAsync(
        string commandText,
        Dictionary<string, object> parameters,
        bool useStoredProcedure,
        bool disableTimeout,
        string connectionStringName,
        CancellationToken cancellationToken)
    {
        parameters ??= new Dictionary<string, object>();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            await using var connection = await CreateConnectionAsync(connectionStringName, cancellationToken);
            await using var cmd = CreateCommand(
                connection,
                commandText,
                parameters,
                useStoredProcedure ? CommandType.StoredProcedure : CommandType.Text,
                disableTimeout);

            try
            {
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing ExecuteNonQueryAsync for {CommandText}", commandText);
                throw;
            }
        });
    }
}
