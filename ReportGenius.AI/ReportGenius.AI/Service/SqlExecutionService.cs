using Dapper;
using Microsoft.Data.SqlClient;

namespace ReportGenius.AI.Service
{
    public class SqlExecutionService
    {
        private readonly IConfiguration _config;

        public SqlExecutionService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<dynamic>> ExecuteAsync(string sql)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return await conn.QueryAsync(sql);
        }
    }
}
