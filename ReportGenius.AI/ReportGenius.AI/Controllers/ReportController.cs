
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
        private readonly string _connectionString;
        private readonly SqlSafetyService _safety;
        private readonly SchemaService _schema;
        private readonly GroqService _groqService;
        private readonly SqlValidatorService _validator;
        public ReportController(
            GeminiService ai,
            DataService data, IConfiguration config,
            SqlSafetyService safety,
            SchemaService schema,
            GroqService groqService, SqlValidatorService validator)
        {
            _ai = ai;
            _data = data;
            _connectionString = config["ConnectionStrings:DefaultConnection"];
            _safety = safety;
            _schema = schema;
            _groqService = groqService;
            _validator = validator;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto req)
        {
            try
            {
                var schema = await _schema.GetSchemaAsync(_connectionString);

                var sql = await _groqService.GenerateSqlAsync(schema, req.Prompt);

                Console.WriteLine("SQL => " + sql);

                if (!_safety.IsSafeQuery(sql))
                    return BadRequest("Unsafe SQL");

                if (!_validator.Validate(sql, schema))
                    return BadRequest("Invalid column name generated");

                var data = (await _data.ExecuteQueryAsync(_connectionString, sql)).ToList();

                return Ok(new
                {
                    sql,
                    count = data.Count,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}