using Dapper;
using Microsoft.Data.SqlClient;

public class DataService
{
    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string connStr, string sql)
    {
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        return await conn.QueryAsync(sql);
    }
}