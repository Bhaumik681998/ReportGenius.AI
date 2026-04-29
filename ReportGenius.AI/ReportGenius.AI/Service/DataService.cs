namespace ReportGenius.AI.Service
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Text.Json;

    public class DataService
    {
        public async Task<string> ExecuteQueryAsync(string connStr, string sql)
        {
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            var result = await conn.QueryAsync(sql);

            return JsonSerializer.Serialize(result);
        }
    }
}
