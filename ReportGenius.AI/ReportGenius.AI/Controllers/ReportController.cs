
using Microsoft.AspNetCore.Mvc;
using ReportGenius.AI.DTOs;
using ReportGenius.AI.Model;
using ReportGenius.AI.Service;
using System.Text.Json;

namespace AIReport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly GeminiService _ai;
        private readonly DataService _data;
        private readonly SqlSafetyService _safety;
        private readonly SchemaService _schema;

        public ReportController(
            GeminiService ai,
            DataService data,
            SqlSafetyService safety,
            SchemaService schema)
        {
            _ai = ai;
            _data = data;
            _safety = safety;
            _schema = schema;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto request)
        {
            try
            {
                // 1. Get schema
                var schema = await _schema.GetSchemaAsync(request.ConnectionString);

                // 2. Generate SQL
                var sql = await _ai.GenerateSqlAsync(schema, request.Prompt);

                Console.WriteLine("SQL: " + sql);

                // 3. Safety check
                if (!_safety.IsSafeQuery(sql))
                    return BadRequest("Unsafe SQL");

                // 4. Execute query
                var dataJson = await _data.ExecuteQueryAsync(request.ConnectionString, sql);

                // 5. Generate report
                var reportJson = await _ai.GenerateReportAsync(dataJson, request.Prompt);

                Console.WriteLine("REPORT: " + reportJson);

                // 6. Safe deserialize
                try
                {
                    var result = JsonSerializer.Deserialize<ReportResult>(reportJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(result);
                }
                catch
                {
                    return Ok(new
                    {
                        raw = reportJson,
                        message = "AI returned invalid JSON - showing raw output"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}