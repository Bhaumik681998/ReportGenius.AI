using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class SqlSyntaxFixService
    {
        public string Fix(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            // 🔹 1. Fix: missing comma between columns
            sql = Regex.Replace(sql,
                @"SELECT\s+([a-zA-Z0-9_]+)\s+([a-zA-Z0-9_]+)\s+FROM",
                "SELECT $1, $2 FROM",
                RegexOptions.IgnoreCase);

            // 🔹 2. Fix: trailing comma before FROM
            sql = Regex.Replace(sql,
                @",\s*FROM",
                " FROM",
                RegexOptions.IgnoreCase);

            // 🔹 3. Fix: double column alias issue
            sql = Regex.Replace(sql,
                @"\b(\w+)\s+(\w+)\s+AS",
                "$1 AS $2",
                RegexOptions.IgnoreCase);

            // 🔹 4. Remove duplicate commas
            sql = Regex.Replace(sql,
                @",\s*,",
                ",",
                RegexOptions.IgnoreCase);

            // 🔹 5. Clean multiple spaces
            sql = Regex.Replace(sql,
                @"\s+",
                " ").Trim();

            return sql;
        }
    }
}
