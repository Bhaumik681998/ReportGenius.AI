using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReportGenius.AI.Helper;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class GroqService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public GroqService(IConfiguration config, HttpClient http)
        {
            _http = http;
            _apiKey = config["Groq:ApiKey"];
            _model = config["Groq:Model"];

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        private async Task<string> CallGroqAsync(string prompt)
        {
            var url = "https://api.groq.com/openai/v1/chat/completions";

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0
            };

            var response = await _http.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(body),
                Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();

            var parsed = JObject.Parse(json);

            if (parsed["error"] != null)
                throw new Exception(parsed["error"]?["message"]?.ToString());

            return parsed["choices"]?[0]?["message"]?["content"]?.ToString();
        }

        public async Task<string> GenerateSqlAsync(string schema, string prompt)
        {
            var raw = await CallGroqAsync($@"
You are SQL Server expert.

STRICT RULES:
- Return ONLY valid SQL
- SQL MUST be executable without error
- EACH column MUST be separated by comma
- WRONG: SELECT Name Age
- CORRECT: SELECT Name, Age
- DO NOT miss commas
- DO NOT write explanation
- DO NOT write text like 'This query...'
- OUTPUT must start with SELECT

Schema:
{schema}

User Request:
{prompt}
");

            return CleanSql(raw);
        }

        private string CleanSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return "";

            // remove markdown
            sql = sql.Replace("```sql", "")
                     .Replace("```", "")
                     .Trim();

            // 🔥 IMPORTANT: Extract SELECT only (ignore text before)
            var match = Regex.Match(sql, @"(SELECT|WITH)\s+[\s\S]*", RegexOptions.IgnoreCase);

            if (!match.Success)
                return "";

            sql = match.Value;

            // remove comments
            sql = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);

            // remove multiple spaces
            sql = Regex.Replace(sql, @"\s+", " ").Trim();

            return sql;
        }
    }
}