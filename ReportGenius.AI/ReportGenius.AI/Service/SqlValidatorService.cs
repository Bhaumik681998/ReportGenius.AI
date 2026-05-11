using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class SqlValidatorService
    {
        public bool IsSafe(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var blocked = new[]
            {
                "DELETE",
                "UPDATE",
                "DROP",
                "TRUNCATE",
                "ALTER",
                "INSERT",
                "EXEC"
            };

            foreach (var word in blocked)
            {
                if (Regex.IsMatch(sql,
                    $@"\b{word}\b",
                    RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }

            return sql.StartsWith("SELECT",
                StringComparison.OrdinalIgnoreCase);
        }
    }
}