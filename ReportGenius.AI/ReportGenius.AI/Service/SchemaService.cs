using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace ReportGenius.AI.Service
{
    public class SchemaService
    {
        public async Task<string> GetSchemaAsync(string connectionString)
        {
            using var conn = new SqlConnection(connectionString);

            var tables = await conn.QueryAsync<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'");

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                var cols = await conn.QueryAsync<string>(
                    $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{table}'");

                sb.AppendLine($"Table: {table}");
                sb.AppendLine("Columns: " + string.Join(", ", cols));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
