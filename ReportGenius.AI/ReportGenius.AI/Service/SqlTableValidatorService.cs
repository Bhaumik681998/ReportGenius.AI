using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class SqlTableValidatorService
    {
        public string Fix(string sql, string schema)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            var tables = ExtractTables(schema);

            var matches = Regex.Matches(sql,
                @"\bFROM\s+(\w+)|\bJOIN\s+(\w+)",
                RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                var table = m.Groups[1].Value;
                if (string.IsNullOrEmpty(table))
                    table = m.Groups[2].Value;

                if (string.IsNullOrEmpty(table)) continue;

                if (!tables.Contains(table, StringComparer.OrdinalIgnoreCase))
                {
                    var best = SmartMatch(table, tables);

                    if (!string.IsNullOrEmpty(best))
                    {
                        sql = Regex.Replace(sql,
                            $@"\b{table}\b",
                            best,
                            RegexOptions.IgnoreCase);
                    }
                }
            }

            return sql;
        }

        private List<string> ExtractTables(string schema)
        {
            var list = new List<string>();

            var matches = Regex.Matches(schema, @"^(\w+):", RegexOptions.Multiline);

            foreach (Match m in matches)
                list.Add(m.Groups[1].Value);

            return list;
        }

        // 🔥 FINAL SMART MATCH (MOST IMPORTANT)
        private string SmartMatch(string input, List<string> tables)
        {
            input = input.ToLower();

            // ✅ 1. Exact match ignore case
            var exact = tables.FirstOrDefault(t =>
                t.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(exact))
                return exact;

            // ✅ 2. Remove "id" suffix (KishoreID → Kishore)
            if (input.EndsWith("id"))
            {
                var baseName = input.Replace("id", "");

                var match = tables.FirstOrDefault(t =>
                    t.ToLower() == baseName);

                if (!string.IsNullOrEmpty(match))
                    return match;
            }

            // ✅ 3. Contains match
            var contains = tables.FirstOrDefault(t =>
                t.ToLower().Contains(input) || input.Contains(t.ToLower()));

            if (!string.IsNullOrEmpty(contains))
                return contains;

            return null;
        }
    }
}
