using Dapper;
using Microsoft.Data.SqlClient;
using ReportGenius.AI.Model;

namespace ReportGenius.AI.Service
{
    public class MetadataCacheService
    {
        private readonly IConfiguration _config;

        public List<TableMetadata> Tables { get; private set; }
            = new();

        public List<RelationshipMetadata> Relationships
            = new();

        public MetadataCacheService(IConfiguration config)
        {
            _config = config;
        }

        public async Task LoadAsync()
        {
            var connStr =
                _config.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);

            var result = await conn.QueryAsync(@"
                SELECT 
                    t.name AS TableName,
                    c.name AS ColumnName,
                    ty.name AS DataType
                FROM sys.tables t
                JOIN sys.columns c
                    ON t.object_id = c.object_id
                JOIN sys.types ty
                    ON c.user_type_id = ty.user_type_id
                ORDER BY t.name");

            Tables = result
                .GroupBy(x => x.TableName)
                .Select(g => new TableMetadata
                {
                    TableName = g.Key,
                    Columns = g.Select(x =>
                        new ColumnMetadata
                        {
                            ColumnName = x.ColumnName,
                            DataType = x.DataType
                        }).ToList()
                }).ToList();

            BuildRelationships();
        }

        private void BuildRelationships()
        {
            foreach (var table in Tables)
            {
                foreach (var col in table.Columns)
                {
                    if (!col.ColumnName.EndsWith("ID",
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var target in Tables)
                    {
                        if (table.TableName == target.TableName)
                            continue;

                        var match = target.Columns.FirstOrDefault(x =>
                            x.ColumnName.Equals(
                                col.ColumnName,
                                StringComparison.OrdinalIgnoreCase));

                        if (match != null)
                        {
                            Relationships.Add(
                                new RelationshipMetadata
                                {
                                    FromTable = table.TableName,
                                    FromColumn = col.ColumnName,
                                    ToTable = target.TableName,
                                    ToColumn = match.ColumnName
                                });
                        }
                    }
                }
            }
        }
    }
}
