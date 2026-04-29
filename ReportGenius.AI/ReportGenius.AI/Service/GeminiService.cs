using System.Text;
using System.Text.Json;

namespace ReportGenius.AI.Service
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"]!;
            _model = config["Gemini:Model"]!;
        }

        private async Task<string> CallGeminiAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await _http.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("RAW GEMINI:");
            Console.WriteLine(json);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Handle error
            if (root.TryGetProperty("error", out var err))
                return $"ERROR: {err.GetProperty("message").GetString()}";

            // Safe parse
            if (root.TryGetProperty("candidates", out var candidates))
            {
                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "";
            }

            return "ERROR: Invalid response";
        }

        public async Task<string> GenerateSqlAsync(string schema, string prompt)
        {
            var raw = await CallGeminiAsync($@"
            You are SQL Server expert.
            
            Schema:
            {schema}
            
            Request:
            {prompt}
            
            Rules:
            - Only SELECT
            - Use real table names
            - No explanation
            ");

            if (raw.StartsWith("ERROR"))
                throw new Exception(raw);

            return CleanSql(raw);
        }

        private string CleanSql(string sql)
        {
            sql = sql.Replace("```sql", "")
                     .Replace("```", "")
                     .Trim();

            int i = sql.ToLower().IndexOf("select");
            return i >= 0 ? sql.Substring(i) : sql;
        }

        public async Task<string> GenerateReportAsync(string data, string prompt)
        {
            var raw = await CallGeminiAsync($@"
            Return ONLY JSON.
            
            {{
              ""narrative"": ""text"",
              ""insights"": [""text""],
              ""chartData"": {{
                ""labels"": [""A""],
                ""datasets"": [
                  {{
                    ""label"": ""text"",
                    ""data"": [10,20],
                    ""backgroundColor"": ""red""
                  }}
                ]
              }},
              ""tableHtml"": ""html"",
              ""recommendation"": ""text""
            }}
            
            User:
            {prompt}
            
            Data:
            {data}
            ");

            if (raw.StartsWith("ERROR"))
                throw new Exception(raw);

            return CleanJson(raw);
        }

        private string CleanJson(string input)
        {
            input = input.Replace("```json", "")
                         .Replace("```", "")
                         .Trim();

            int s = input.IndexOf('{');
            int e = input.LastIndexOf('}');

            if (s >= 0 && e > s)
                return input.Substring(s, e - s + 1);

            return "{}";
        }
    }
}
