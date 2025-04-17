using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Common.Core.Data;

/// <summary>
/// Data Converters
/// </summary>
public static class DataConverters
{
    /// <summary>
    /// Get default JSON serialization options with camelCase naming
    /// </summary>
    private static JsonSerializerOptions GetDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Converts a DataTable to JSON string
    /// </summary>
    public static string DataTableToJson(DataTable table)
    {
        if (table == null)
            return "null";

        // Convert DataTable to a list of row dictionaries to avoid System.Type serialization issues
        var rows = new List<Dictionary<string, object>>();

        foreach (DataRow dr in table.Rows)
        {
            var row = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
            }
            rows.Add(row);
        }

        return JsonSerializer.Serialize(rows, GetDefaultOptions());
    }

    /// <summary>
    /// Converts a DataSet to JSON string
    /// </summary>
    public static string DataSetToJson(DataSet ds, bool oldConverter = false)
    {
        if (ds == null)
            return "null";

        if (oldConverter)
        {
            var root = new List<object>();
            foreach (DataTable dt in ds.Tables)
            {
                var tableRows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in dt.Rows)
                {
                    var row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                    }
                    tableRows.Add(row);
                }
                root.Add(tableRows);
            }
            return JsonSerializer.Serialize(root, GetDefaultOptions());
        }

        // Create a dictionary of table name to rows for modern format
        var dataSetDict = new Dictionary<string, List<Dictionary<string, object>>>();

        foreach (DataTable dt in ds.Tables)
        {
            var tableRows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                }
                tableRows.Add(row);
            }
            dataSetDict[dt.TableName] = tableRows;
        }

        return JsonSerializer.Serialize(dataSetDict, GetDefaultOptions());
    }

    /// <summary>
    /// Converts a JSON string to DataTable
    /// </summary>
    public static DataTable JsonToDataTable(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new DataTable();

        try
        {
            // Deserialize to List of Dictionaries
            var rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

            if (rows == null || rows.Count == 0)
                return new DataTable();

            // Create DataTable structure based on the first row
            var dt = new DataTable();
            foreach (var key in rows[0].Keys)
            {
                dt.Columns.Add(key);
            }

            // Populate the DataTable
            foreach (var row in rows)
            {
                var dataRow = dt.NewRow();
                foreach (var kvp in row)
                {
                    dataRow[kvp.Key] = GetValueFromJsonElement(kvp.Value);
                }
                dt.Rows.Add(dataRow);
            }

            return dt;
        }
        catch
        {
            // Try alternate format (dictionary of arrays)
            try
            {
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                // If it's a single object, create one row
                if (root.ValueKind == JsonValueKind.Object)
                {
                    var dt = new DataTable();
                    var row = dt.NewRow();

                    foreach (var property in root.EnumerateObject())
                    {
                        dt.Columns.Add(property.Name);
                        row[property.Name] = GetValueFromJsonElement(property.Value);
                    }

                    dt.Rows.Add(row);
                    return dt;
                }
            }
            catch
            {
                // Ignore and return empty table as fallback
            }

            return new DataTable();
        }
    }

    /// <summary>
    /// Helper method to convert JsonElement to appropriate .NET type
    /// </summary>
    private static object GetValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => DBNull.Value,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number when element.TryGetInt32(out var i) => i,
            JsonValueKind.Number when element.TryGetInt64(out var l) => l,
            JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.String when DateTime.TryParse(element.GetString(), out var dt) => dt,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Array => element.ToString(), // Just serialize arrays as strings
            JsonValueKind.Object => element.ToString(), // Just serialize objects as strings
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Serialize a dictionary to JSON string directly preserving original values
    /// </summary>

    public static string SerializeDictionary(Dictionary<string, object> data)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(data,
            new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver { NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy() },
                Formatting = (Newtonsoft.Json.Formatting)Formatting.Indented
            });
    }
    /// <summary>
    /// Helper method to sanitize values for serialization
    /// </summary>
    private static object SanitizeValueForSerialization(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        // Handle DataTable
        if (value is DataTable dt)
            return ConvertDataTableToList(dt);

        // Handle DataSet
        if (value is DataSet ds)
        {
            var result = new Dictionary<string, object>();
            foreach (DataTable table in ds.Tables)
            {
                result[table.TableName] = ConvertDataTableToList(table);
            }
            return result;
        }

        // Handle Type objects (convert to string)
        if (value is Type)
            return value.ToString();

        // Check if it's an array
        if (value is Array arr)
        {
            var result = new List<object>();
            foreach (var item in arr)
            {
                result.Add(SanitizeValueForSerialization(item));
            }
            return result;
        }

        // Handle generic collections but preserve their actual values
        if (value is IEnumerable collection && !(value is string))
        {
            var result = new List<object>();
            foreach (var item in collection)
            {
                result.Add(SanitizeValueForSerialization(item));
            }
            return result;
        }

        // Return primitives and other simple types as is
        return value;
    }

    /// <summary>
    /// Helper method to convert DataTable to a list of dictionaries
    /// </summary>
    private static List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
    {
        var result = new List<Dictionary<string, object>>();

        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            }
            result.Add(dict);
        }

        return result;
    }

    /// <summary>
    /// Convert dictionary to string representation
    /// </summary>
    public static string DictionaryToString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null)
            return "{}";

        return "{" + string.Join(", ", dictionary.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray()) + "}";
    }

    /// <summary>
    /// Generic method to serialize any object to JSON
    /// </summary>
    public static string SerializeObjectCustom<T>(object data)
    {
        if (data == null)
            return "null";

        // If it's a DataTable or DataSet, use our specialized methods
        if (data is DataTable dt)
            return DataTableToJson(dt);

        if (data is DataSet ds)
            return DataSetToJson(ds);

        // For other objects, apply standard serialization
        try
        {
            return JsonSerializer.Serialize(data, GetDefaultOptions());
        }
        catch (JsonException)
        {
            // If standard serialization fails, try to sanitize the object first
            var sanitizedData = SanitizeValueForSerialization(data);
            return JsonSerializer.Serialize(sanitizedData, GetDefaultOptions());
        }
    }
}
