namespace ReportGenius.AI.Service
{
    public class SqlSafetyService
    {
        public bool IsSafeQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var words = sql.ToLower()
                           .Split(' ', '(', ')', ',', ';');

            string[] blocked =
            {
                "drop","delete","truncate",
                "update","insert","alter",
                "exec","execute"
            };

            return !blocked.Any(w => words.Contains(w));
        }
    }
}