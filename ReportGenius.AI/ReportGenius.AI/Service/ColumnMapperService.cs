using System.Text.RegularExpressions;

public class ColumnMapperService
{
    public string FixColumns(string sql, string schema)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return sql;

        var columns = ExtractColumns(schema);

        // 🔥 Extract all column names used in SQL
        var usedColumns = ExtractUsedColumns(sql);

        foreach (var col in usedColumns)
        {
            // જો column schema માં નથી → replace કરવું
            if (!columns.Contains(col, StringComparer.OrdinalIgnoreCase))
            {
                var bestMatch = FindBestMatch(col, columns);

                if (!string.IsNullOrEmpty(bestMatch))
                {
                    sql = Regex.Replace(sql,
                        $@"\b{col}\b",
                        bestMatch,
                        RegexOptions.IgnoreCase);
                }
            }
        }

        return sql;
    }

    // 🔹 schema માંથી columns કાઢો
    private List<string> ExtractColumns(string schema)
    {
        var list = new List<string>();

        var matches = Regex.Matches(schema, @"-\s*(\w+)");

        foreach (Match m in matches)
        {
            list.Add(m.Groups[1].Value);
        }

        return list;
    }

    // 🔹 SQL માંથી used columns કાઢો
    private List<string> ExtractUsedColumns(string sql)
    {
        var list = new List<string>();

        var matches = Regex.Matches(sql, @"\b[A-Za-z_][A-Za-z0-9_]*\b");

        foreach (Match m in matches)
        {
            var word = m.Value;

            // skip SQL keywords
            if (IsSqlKeyword(word)) continue;

            list.Add(word);
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // 🔹 Best match logic (IMPORTANT)
    private string FindBestMatch(string input, List<string> columns)
    {
        input = input.ToLower();

        // 1. Exact contains match
        var match = columns.FirstOrDefault(c =>
            c.ToLower().Contains(input) || input.Contains(c.ToLower()));

        if (!string.IsNullOrEmpty(match))
            return match;

        // 2. ID special handling
        if (input == "id")
        {
            return columns.FirstOrDefault(c =>
                c.EndsWith("ID", StringComparison.OrdinalIgnoreCase));
        }

        // 3. Name special handling
        if (input == "name")
        {
            return columns.FirstOrDefault(c =>
                c.EndsWith("Name", StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private bool IsSqlKeyword(string word)
    {
        string[] keywords =
        {
            "select","from","where","join","on","and","or",
            "top","inner","left","right","as","order","by"
        };

        return keywords.Contains(word.ToLower());
    }
}