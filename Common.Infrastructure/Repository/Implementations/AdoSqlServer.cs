using Common.Infrastructure.Repository.Interfaces;
using Common.Infrastructure.Utils.Database;

namespace Common.Infrastructure.Repository.Implementations;

public class AdoSqlServer(IConfiguration configuration) : IAdoSqlServer
{
    public async Task<List<T>> DynamicListAsync<T>(string spname, Dictionary<string, object> parameters,
        string connectionStringName)
    {
        var response = new List<T>();
        await using var conn =
            new SqlConnection(configuration.GetConnectionString(connectionStringName ?? "DefaultConnection"));
        await using var cmd = new SqlCommand(spname, conn);
        var wasOpen = cmd.Connection.State == ConnectionState.Open;
        try
        {
            parameters ??= new Dictionary<string, object>();

            if (parameters.Count > 0)
            {
                foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    ParametersUtilsSqlServer.AddSqlParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            cmd.CommandType = CommandType.StoredProcedure;
            if (!wasOpen)
            {
                await conn.OpenAsync();
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            response = DbDataReaderMapper.ToList<T>(reader);
        }
        finally
        {
            if (!wasOpen && cmd.Connection?.State == ConnectionState.Open)
            {
                await cmd.Connection.CloseAsync();
            }
        }

        return response;
    }

    public async Task<DataSet> GetDataSetAsync(string spname, Dictionary<string, object> parameters, bool withTableNames,
        bool timeout, string connectionStringName)
    {
        var ds = new DataSet();
        connectionStringName = string.IsNullOrEmpty(connectionStringName) ? "DefaultConnection" : connectionStringName;
        await using var conn =
            new SqlConnection(configuration.GetConnectionString(connectionStringName));
        await using var cmd = new SqlCommand(spname, conn);
        var wasOpen = cmd.Connection?.State == ConnectionState.Open;
        try
        {
            parameters ??= new Dictionary<string, object>();

            if (parameters.Count > 0)
            {
                foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    ParametersUtilsSqlServer.AddSqlParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            const string outParam = "@tableNames";
            if (withTableNames)
            {
                ParametersUtilsSqlServer.AddSqlParameterOut(cmd, outParam, SqlDbType.VarChar, 500);
            }

            if (!timeout)
            {
                cmd.CommandTimeout = 0;
            }

            cmd.CommandType = CommandType.StoredProcedure;
            if (!wasOpen)
            {
                await conn.OpenAsync();
            }

            using (var adapter = new SqlDataAdapter())
            {
                adapter.SelectCommand = cmd;
                ds = new DataSet();
                await Task.Run(() => adapter.Fill(ds));
            }

            if (withTableNames)
            {
                var tableNames = ParametersUtilsSqlServer.GetParameter(cmd, outParam).ToString()?.Split(',');
                await Task.Run(() =>
                {
                    if (tableNames != null)
                    {
                        Parallel.ForEach(tableNames, (tableName, state, index) =>
                        {
                            if (!string.IsNullOrEmpty(tableName))
                            {
                                ds.Tables[(int)index].TableName = tableName;
                            }
                        });
                    }
                });
            }
        }
        finally
        {
            if (!wasOpen && cmd.Connection?.State == ConnectionState.Open)
            {
                await cmd.Connection.CloseAsync();
            }
        }

        return ds;
    }

    public async Task<DataTable> GetDataTableAsync(string spname, Dictionary<string, object> parameters,
        string connectionStringName)
    {
        var dt = new DataTable();
        connectionStringName = string.IsNullOrEmpty(connectionStringName) ? "DefaultConnection" : connectionStringName;
        await using var conn =
            new SqlConnection(configuration.GetConnectionString(connectionStringName));
        await using var cmd = new SqlCommand(spname, conn);
        var wasOpen = cmd.Connection.State == ConnectionState.Open;
        try
        {
            parameters ??= [];

            if (parameters.Count > 0)
            {
                foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    ParametersUtilsSqlServer.AddSqlParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            cmd.CommandType = CommandType.StoredProcedure;
            if (!wasOpen)
            {
                await conn.OpenAsync();
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            dt.Load(reader);
        }
        finally
        {
            if (!wasOpen && cmd.Connection?.State == ConnectionState.Open)
            {
                await cmd.Connection.CloseAsync();
            }
        }

        return dt;
    }

    public async Task<int> OnlyExecuteAsync(string query, Dictionary<string, object> parameters,
        bool useStoredProcedure, bool timeout, string connectionStringName)
    {
        connectionStringName = string.IsNullOrEmpty(connectionStringName) ? "DefaultConnection" : connectionStringName;
        await using var conn =
            new SqlConnection(configuration.GetConnectionString(connectionStringName));
        await using var cmd = new SqlCommand(query, conn);
        var wasOpen = cmd.Connection.State == ConnectionState.Open;
        try
        {
            parameters ??= new Dictionary<string, object>();

            if (parameters.Count > 0)
            {
                foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    ParametersUtilsSqlServer.AddSqlParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            if (!timeout)
            {
                cmd.CommandTimeout = 0;
            }

            cmd.CommandType = useStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
            if (!wasOpen)
            {
                await conn.OpenAsync();
            }

            return await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            if (!wasOpen && cmd.Connection?.State == ConnectionState.Open)
            {
                await cmd.Connection.CloseAsync();
            }
        }
    }

    public async Task<T> SpExecuteAsync<T>(string spname, Dictionary<string, object> parameters, bool timeout,
        string connectionStringName)
        where T : class, new()
    {
        var response = new T();
        await using var conn =
            new SqlConnection(configuration.GetConnectionString(connectionStringName ?? "DefaultConnection"));
        await using var cmd = new SqlCommand(spname, conn);
        var wasOpen = cmd.Connection.State == ConnectionState.Open;
        try
        {
            parameters ??= new Dictionary<string, object>();

            if (parameters.Count > 0)
            {
                foreach (var parameter in parameters.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    ParametersUtilsSqlServer.AddSqlParameter(cmd, parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            if (!timeout)
            {
                cmd.CommandTimeout = 0;
            }

            cmd.CommandType = CommandType.StoredProcedure;
            if (!wasOpen)
            {
                await conn.OpenAsync();
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // Reflection to access properties dynamically
                var properties = typeof(T).GetProperties();
                foreach (var property in properties)
                {
                    // Check if property name matches a column name (case-insensitive)
                    var columnName = reader.GetSchemaTable()
                        ?.Rows.Cast<DataRow>()
                        .Where(row =>
                            row["ColumnName"].ToString()!.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                        .Select(row => row["ColumnName"].ToString())
                        .FirstOrDefault();

                    if (columnName == null)
                    {
                        continue;
                    }

                    var index = reader.GetOrdinal(columnName);
                    if (index != -1 && !reader.IsDBNull(index))
                    {
                        // Set property value based on data type
                        property.SetValue(response, reader.GetValue(index));
                    }
                }
            }
        }
        finally
        {
            if (!wasOpen && cmd.Connection?.State == ConnectionState.Open)
            {
                await cmd.Connection.CloseAsync();
            }
        }

        return response;
    }
}
