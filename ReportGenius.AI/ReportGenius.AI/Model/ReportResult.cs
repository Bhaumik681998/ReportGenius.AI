namespace ReportGenius.AI.Model
{
    //public class ReportResult
    //{
    //    public string? Narrative { get; set; }
    //    public List<string> Insights { get; set; } = new List<string>();
    //    public ChartData ChartData { get; set; } = new ChartData();
    //    public string? TableHtml { get; set; }
    //    public string? Recommendation { get; set; }
    //}
    public class ReportResult
    {
        public string? Narrative { get; set; }
        public List<string>? Insights { get; set; } = new List<string>();
        public object? ChartData { get; set; }
        public string? TableHtml { get; set; }
        public string? Recommendation { get; set; }
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class ChartDataset
    {
        public string? Label { get; set; }
        public List<decimal> Data { get; set; } = new List<decimal>();
        public string? BackgroundColor { get; set; }
    }
}
