using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class TableMapperService
    {
        public string FixTables(string sql, string schema)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            var tables = ExtractTables(schema);

            // SQL માંથી tables શોધો
            var matches = Regex.Matches(sql, @"\bFROM\s+(\w+)|\bJOIN\s+(\w+)", RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                var table = m.Groups[1].Value;
                if (string.IsNullOrEmpty(table))
                    table = m.Groups[2].Value;

                if (string.IsNullOrEmpty(table)) continue;

                // જો schema માં નથી → fix કરો
                if (!tables.Contains(table, StringComparer.OrdinalIgnoreCase))
                {
                    var bestMatch = FindBestTableMatch(table, tables);

                    if (!string.IsNullOrEmpty(bestMatch))
                    {
                        sql = Regex.Replace(sql,
                            $@"\b{table}\b",
                            bestMatch,
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
            {
                list.Add(m.Groups[1].Value);
            }

            return list;
        }

        private string FindBestTableMatch(string input, List<string> tables)
        {
            input = input.ToLower();

            return tables.FirstOrDefault(t =>
                t.ToLower().Contains(input) || input.Contains(t.ToLower()));
        }
    }
}
