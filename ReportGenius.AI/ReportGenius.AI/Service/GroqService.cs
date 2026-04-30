using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

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

        public async Task<string> GenerateSqlAsync(string schema, string prompt)
        {
            var systemPrompt = $@"
You are a SQL Server expert.

Database Schema:
{schema}

STRICT RULES:
- Use ONLY columns from schema
- DO NOT guess column names
- Return ONLY SQL query
- No explanation
- Only SELECT query
";

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0
            };

            var response = await _http.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(JsonConvert.SerializeObject(body),
                Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();

            var parsed = JObject.Parse(json);

            if (parsed["error"] != null)
                throw new Exception(parsed["error"]?["message"]?.ToString());

            var sql = parsed["choices"]?[0]?["message"]?["content"]?.ToString();

            return CleanSql(sql);
        }

        private string CleanSql(string sql)
        {
            sql = sql.Replace("```sql", "", StringComparison.OrdinalIgnoreCase)
                     .Replace("```", "")
                     .Trim();

            var lower = sql.ToLower();

            int selectIndex = lower.IndexOf("select");
            int withIndex = lower.IndexOf("with");

            int start = selectIndex >= 0 ? selectIndex : withIndex;

            if (start >= 0)
                sql = sql.Substring(start);

            int semicolon = sql.IndexOf(";");
            if (semicolon > 0)
                sql = sql.Substring(0, semicolon);

            return sql.Trim();
        }
    }
}
