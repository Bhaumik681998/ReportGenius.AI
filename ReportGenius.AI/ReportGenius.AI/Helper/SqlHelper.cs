using System.Text.RegularExpressions;

namespace ReportGenius.AI.Helper
{
    public static class SqlHelper
    {
        public static string ProcessSql(string sql, string prompt)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return "";

            // Remove markdown
            sql = sql.Replace("```sql", "")
                     .Replace("```", "")
                     .Trim();

            // Extract SELECT
            var match = Regex.Match(sql, @"select[\s\S]*", RegexOptions.IgnoreCase);
            if (!match.Success)
                return "";

            sql = match.Value;

            // Remove after ;
            int i = sql.IndexOf(";");
            if (i > 0)
                sql = sql.Substring(0, i);

            // 🔥 FIX 1: REMOVE ORDER BY
            if (!prompt.ToLower().Contains("order"))
            {
                sql = Regex.Replace(sql, @"order\s+by[\s\S]*", "", RegexOptions.IgnoreCase);
            }

            // 🔥 FIX 2: BIT VALUE
            sql = Regex.Replace(sql, @"=\s*'active'", "= 1", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"=\s*'inactive'", "= 0", RegexOptions.IgnoreCase);

            // 🔥 FIX 3: SELECT * (BUT KEEP TOP)
            if (prompt.ToLower().Contains("record") || prompt.ToLower().Contains("list"))
            {
                sql = Regex.Replace(sql,
                    @"select\s+top\s+(\d+)\s+.+?\s+from",
                    m => $"SELECT TOP {m.Groups[1].Value} * FROM",
                    RegexOptions.IgnoreCase);

                if (!sql.ToLower().Contains("top"))
                {
                    sql = Regex.Replace(sql,
                        @"select\s+.+?\s+from",
                        "SELECT * FROM",
                        RegexOptions.IgnoreCase);
                }
            }

            // 🔥 FIX 4: REMOVE INVALID FILTERS
            sql = Regex.Replace(sql, @"\bZone\s*=\s*'[^']*'\s*(AND)?", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\bArea\s*=\s*'[^']*'\s*(AND)?", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\bId\s*=\s*'[^']*'\s*(AND)?", "", RegexOptions.IgnoreCase);

            // 🔥 FIX 5: REMOVE EXTRA AND
            sql = Regex.Replace(sql, @"WHERE\s+AND", "WHERE", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"AND\s+AND", "AND", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"WHERE\s*$", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"AND\s*$", "", RegexOptions.IgnoreCase);

            // 🔥 FIX 6: CLEAN DOUBLE WHERE
            sql = Regex.Replace(sql, @"WHERE\s+WHERE", "WHERE", RegexOptions.IgnoreCase);

            // 🔥 FIX 7: FINAL CLEAN
            sql = Regex.Replace(sql, @"\s+", " ");

            return sql.Trim();
        }
    }
}