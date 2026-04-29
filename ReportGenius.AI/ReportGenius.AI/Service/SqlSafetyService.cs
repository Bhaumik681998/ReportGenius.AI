namespace ReportGenius.AI.Service
{
    public class SqlSafetyService
    {
        public bool IsSafeQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            sql = sql.ToLower().Trim();

            if (!sql.StartsWith("select"))
                return false;

            string[] blocked = {
                "insert","update","delete","drop","alter","truncate","exec","xp_"
            };

            return !blocked.Any(x => sql.Contains(x));
        }
    }
}
