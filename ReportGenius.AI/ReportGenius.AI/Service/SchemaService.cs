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

            var result = await conn.QueryAsync(@"
        SELECT 
            t.name AS TableName,
            c.name AS ColumnName,
            ty.name AS DataType
        FROM sys.tables t
        JOIN sys.columns c ON t.object_id = c.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        ORDER BY t.name
    ");

            var sb = new StringBuilder();

            var grouped = result.GroupBy(x => x.TableName);

            foreach (var table in grouped)
            {
                sb.AppendLine($"{table.Key}:");

                foreach (var col in table)
                {
                    sb.AppendLine($"  - {col.ColumnName} ({col.DataType})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}