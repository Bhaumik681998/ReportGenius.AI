using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace ReportGenius.AI.Service
{
    public class SchemaService
    {
        public async Task<string> GetSchemaAsync(string conn)
        {
            using var db = new SqlConnection(conn);

            var tables = await db.QueryAsync<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");

            var sb = new StringBuilder();

            foreach (var t in tables)
            {
                var cols = await db.QueryAsync<string>(
                    $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{t}'");

                sb.AppendLine($"Table: {t} ({string.Join(", ", cols)})");
            }

            return sb.ToString();
        }
    }
}
