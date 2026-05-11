namespace ReportGenius.AI.Model
{
    public class TableMetadata
    {
        public string? TableName { get; set; }

        public List<ColumnMetadata> Columns { get; set; }
            = new();
    }

    public class ColumnMetadata
    {
        public string? ColumnName { get; set; }
        public string? DataType { get; set; }
    }

    public class RelationshipMetadata
    {
        public string? FromTable { get; set; }

        public string? FromColumn { get; set; }
        public string? ToTable { get; set; }
        public string? ToColumn { get; set; }
    }

    public class PromptIntent
    {
        public List<string> Tables { get; set; }= new();
        public List<string> Filters { get; set; }= new();
        public int? Top { get; set; }
    }
}
