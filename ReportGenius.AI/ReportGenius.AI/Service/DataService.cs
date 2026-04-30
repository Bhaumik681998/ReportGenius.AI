namespace ReportGenius.AI.Service
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Text.Json;

    public class DataService
    {
        public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string conn, string sql)
        {
            using var db = new SqlConnection(conn);
            return await db.QueryAsync(sql);
        }
    }
}
