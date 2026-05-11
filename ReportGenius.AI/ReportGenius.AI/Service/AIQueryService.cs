using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class AIQueryService
    {
        private readonly HttpClient _http;

        public AIQueryService(HttpClient http)
        {
            _http = http;

            _http.Timeout =
                TimeSpan.FromMinutes(5);
        }

        public async Task<string> GenerateSqlAsync(
            string schema,
            string prompt)
        {
            var systemPrompt = @"
            You are Microsoft SQL Server Expert.
            
            STRICT RULES:
            - Return ONLY SQL
            - Use SQL Server syntax
            - Use TOP instead of LIMIT
            - Never explain
            - Never generate invalid table
            - Never generate invalid column
            - Always use JOIN when needed
            ";
            
                        var userPrompt = $@"
            SCHEMA:
            {schema}
            
            USER REQUEST:
            {prompt}
            ";

            var body = new
            {
                model = "qwen2.5-coder-7b-instruct",

                messages = new[]
                {
                    new
                    {
                        role="system",
                        content=systemPrompt
                    },

                    new
                    {
                        role="user",
                        content=userPrompt
                    }
                },

                temperature = 0.1,

                max_tokens = 300
            };

            var json =
                JsonConvert.SerializeObject(body);

            var response =
                await _http.PostAsync(
                    "http://127.0.0.1:1234/v1/chat/completions",

                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"));

            var text =
                await response.Content.ReadAsStringAsync();

            var parsed = JObject.Parse(text);

            var sql =
                parsed["choices"]?[0]?["message"]?
                ["content"]?.ToString();

            return Clean(sql);
        }

        private string Clean(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return "";

            sql = sql.Replace("```sql", "")
                .Replace("```", "");

            var match = Regex.Match(sql,
                @"(SELECT|WITH)[\s\S]*",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return "";

            return match.Value.Trim();
        }
    }
}
