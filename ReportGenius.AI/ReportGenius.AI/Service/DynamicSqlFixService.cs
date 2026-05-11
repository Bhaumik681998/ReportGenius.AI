using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class DynamicSqlFixService
    {
        private readonly IConfiguration _config;

        public DynamicSqlFixService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> FixColumnsAsync(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            // ✅ Load REAL DB columns
            var dbColumns = await GetAllDatabaseColumnsAsync();

            // ✅ Extract words from SQL
            var sqlWords = ExtractSqlWords(sql);

            foreach (var word in sqlWords)
            {
                // Skip SQL keywords
                if (IsSqlKeyword(word))
                    continue;

                // Already valid
                if (dbColumns.Any(x =>
                    x.Equals(word,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Find best matching column
                var bestMatch = FindBestMatch(word, dbColumns);

                if (!string.IsNullOrWhiteSpace(bestMatch))
                {
                    Console.WriteLine(
                        $"COLUMN FIX: {word} => {bestMatch}");

                    sql = Regex.Replace(
                        sql,
                        $@"\b{Regex.Escape(word)}\b",
                        bestMatch,
                        RegexOptions.IgnoreCase);
                }
            }

            return sql;
        }

        // ✅ Load all DB columns dynamically
        private async Task<List<string>> GetAllDatabaseColumnsAsync()
        {
            var columns = new List<string>();

            var connectionString =
                _config.GetConnectionString("DefaultConnection");

            using var con = new SqlConnection(connectionString);

            await con.OpenAsync();

            var query = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS";

            using var cmd = new SqlCommand(query, con);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var column = reader["COLUMN_NAME"]?.ToString();

                if (!string.IsNullOrWhiteSpace(column))
                {
                    columns.Add(column);
                }
            }

            return columns.Distinct(
                StringComparer.OrdinalIgnoreCase).ToList();
        }

        // ✅ Extract SQL words
        private List<string> ExtractSqlWords(string sql)
        {
            var words = new List<string>();

            var matches = Regex.Matches(
                sql,
                @"\b[A-Za-z_][A-Za-z0-9_]*\b");

            foreach (Match match in matches)
            {
                var word = match.Value;

                if (!words.Contains(word,
                    StringComparer.OrdinalIgnoreCase))
                {
                    words.Add(word);
                }
            }

            return words;
        }

        // ✅ SQL keywords
        private bool IsSqlKeyword(string word)
        {
            var keywords = new[]
            {
                "SELECT","FROM","WHERE","AND","OR",
                "TOP","INNER","LEFT","RIGHT","JOIN",
                "ON","ORDER","BY","GROUP","DESC",
                "ASC","AS","IN","NOT","NULL",
                "LIKE","COUNT","SUM","AVG","MIN",
                "MAX","DISTINCT"
            };

            return keywords.Contains(
                word.ToUpper());
        }

        // ✅ Find nearest matching column
        private string FindBestMatch(
            string input,
            List<string> dbColumns)
        {
            string bestMatch = "";
            int bestScore = int.MaxValue;

            foreach (var column in dbColumns)
            {
                int score = LevenshteinDistance(
                    input.ToLower(),
                    column.ToLower());

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMatch = column;
                }
            }

            // ✅ Accept close matches only
            return bestScore <= 3
                ? bestMatch
                : "";
        }

        // ✅ Levenshtein Distance
        private int LevenshteinDistance(
            string s,
            string t)
        {
            int[,] d =
                new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost =
                        s[i - 1] == t[j - 1] ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(
                            d[i - 1, j] + 1,
                            d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
    }
}