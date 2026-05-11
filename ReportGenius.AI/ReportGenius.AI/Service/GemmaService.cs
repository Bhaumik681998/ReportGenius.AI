using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class GemmaService
    {
        private readonly HttpClient _http;

        public GemmaService(HttpClient http)
        {
            _http = http;

            // ✅ Increase timeout
            _http.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<string> GenerateSqlAsync(string schema, string prompt)
        {
            try
            {
                // ✅ LM Studio URL
                var url = "http://127.0.0.1:1234/v1/chat/completions";

                // ✅ Strong SQL Prompt
                var systemPrompt = @"
                You are a Microsoft SQL Server expert.
                
                STRICT RULES:
                - Return ONLY valid SQL Server query
                - Use ONLY tables from provided schema
                - NEVER invent table names
                - NEVER invent column names
                - If table not found, use closest matching table
                - Query MUST start with SELECT
                - Use TOP instead of LIMIT
                - Never explain anything
                ";

                // ✅ Reduce schema size for performance
                if (!string.IsNullOrWhiteSpace(schema) && schema.Length > 5000)
                {
                    schema = schema.Substring(0, 5000);
                }

                var userPrompt = $@"
Database Schema:
{schema}

User Request:
{prompt}
";

                // ✅ Request body
                var body = new
                {
                    model = "gemma-2-2b-it",

                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = systemPrompt
                        },
                        new
                        {
                            role = "user",
                            content = userPrompt
                        }
                    },

                    temperature = 0.1,

                    max_tokens = 150
                };

                // ✅ Convert body to JSON
                var jsonBody = JsonConvert.SerializeObject(body);

                Console.WriteLine("==================================");
                Console.WriteLine("CALLING LM STUDIO...");
                Console.WriteLine("==================================");

                // ✅ API Call
                var response = await _http.PostAsync(
                    url,
                    new StringContent(
                        jsonBody,
                        Encoding.UTF8,
                        "application/json")
                );

                // ✅ Read response
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine("==================================");
                Console.WriteLine("LM STUDIO RESPONSE:");
                Console.WriteLine(json);
                Console.WriteLine("==================================");

                // ✅ Check API success
                response.EnsureSuccessStatusCode();

                // ✅ Parse JSON
                var parsed = JObject.Parse(json);

                // ✅ Extract content
                var text = parsed["choices"]?[0]?["message"]?["content"]?.ToString();

                Console.WriteLine("==================================");
                Console.WriteLine("RAW AI SQL:");
                Console.WriteLine(text);
                Console.WriteLine("==================================");

                // ✅ Clean SQL
                var cleanSql = CleanSql(text);

                Console.WriteLine("==================================");
                Console.WriteLine("FINAL CLEAN SQL:");
                Console.WriteLine(cleanSql);
                Console.WriteLine("==================================");

                // ✅ Save SQL for debugging
                File.WriteAllText("last-sql.txt", cleanSql);

                return cleanSql;
            }
            catch (Exception ex)
            {
                Console.WriteLine("==================================");
                Console.WriteLine("GEMMA ERROR:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==================================");

                throw new Exception("Gemma AI Error: " + ex.Message);
            }
        }

        // ✅ SQL Cleaner
        private string CleanSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return "";

            // ✅ Remove markdown
            sql = sql.Replace("```sql", "")
                     .Replace("```", "")
                     .Trim();

            // ✅ Remove think tags
            sql = Regex.Replace(
                sql,
                @"<think>[\s\S]*?</think>",
                "",
                RegexOptions.IgnoreCase);

            // ✅ Extract SQL only
            var match = Regex.Match(
                sql,
                @"(SELECT|WITH)\s+[\s\S]*?(?=;|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return "";

            sql = match.Value.Trim();

            // ✅ Remove SQL comments
            sql = Regex.Replace(
                sql,
                @"--.*?$",
                "",
                RegexOptions.Multiline);

            // ✅ Remove multiple spaces
            sql = Regex.Replace(sql, @"\s+", " ");

            // ✅ Common SQL Fixes
            sql = sql.Replace(" ,", ",");
            sql = sql.Replace("( ", "(");
            sql = sql.Replace(" )", ")");

            // ✅ Remove LIMIT (SQL Server fix)
            sql = Regex.Replace(
                sql,
                @"LIMIT\s+\d+",
                "",
                RegexOptions.IgnoreCase);

            // ✅ Ensure SELECT exists
            if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                && !sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            return sql.Trim();
        }
    }
}