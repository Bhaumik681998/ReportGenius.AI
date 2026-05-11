using System.Text;

namespace ReportGenius.AI.Service
{
    public class RelevantSchemaService
    {
        private readonly MetadataCacheService _cache;
        public RelevantSchemaService(MetadataCacheService cache)
        {
            _cache = cache;
        }

        public string Build(List<string> tables)
        {
            var sb = new StringBuilder();

            foreach (var table in _cache.Tables.Where(x => tables.Contains(x.TableName)))
            {
                sb.AppendLine($"TABLE: {table.TableName}");

                foreach (var col in table.Columns)
                {
                    sb.AppendLine(
                        $"- {col.ColumnName}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}
