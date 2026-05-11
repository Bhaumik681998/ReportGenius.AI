
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ReportGenius.AI.DTOs;
using ReportGenius.AI.Helper;
using ReportGenius.AI.Model;
using ReportGenius.AI.Service;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIReport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly DataService _data;
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly SqlSafetyService _safety;
        private readonly SchemaService _schema;
        private readonly GroqService _groqService;
        private readonly SqlValidatorService _validator;
        private readonly ColumnMapperService _mapper;
        private readonly PromptNormalizerService _normalizer;
        private readonly SqlJoinFixService _joinFix;
        private readonly TableMapperService _tableMapper;
        private readonly SqlSyntaxFixService _syntaxFix;
        private readonly SqlTableValidatorService _tableValidator;
        private readonly GemmaService _gemma;
        private readonly DynamicSqlFixService _dynamicFix;
        private readonly SemanticMapperService _semantic;
        private readonly PromptUnderstandingService _intent;
        private readonly RelevantSchemaService _relevantSchemaService;
        private readonly AIQueryService _aIQueryService;
        private readonly SqlExecutionService _sql;
        public ReportController(
            GeminiService geminiService,
            DataService data, IConfiguration config,
            SqlSafetyService safety,
            SchemaService schema,
            GroqService groqService, SqlValidatorService validator, ColumnMapperService mapper,
            PromptNormalizerService normalizer,
            SqlJoinFixService joinFix, TableMapperService tableMapper,
            SqlSyntaxFixService syntaxFix, SqlTableValidatorService tableValidator,
            GemmaService gemma, DynamicSqlFixService dynamicFix,
            SemanticMapperService semantic, PromptUnderstandingService intent, 
            RelevantSchemaService relevantSchemaService, AIQueryService aIQueryService,
            SqlExecutionService sqlExecutionService)
        {
            _geminiService = geminiService; _data = data;
            _connectionString = config["ConnectionStrings:DefaultConnection"];
            _config = config; _safety = safety; _schema = schema; _groqService = groqService;
            _validator = validator; _mapper = mapper; _normalizer = normalizer;
            _joinFix = joinFix; _tableMapper = tableMapper; _syntaxFix = syntaxFix;
            _tableValidator = tableValidator; _gemma = gemma; _dynamicFix = dynamicFix;
            _semantic = semantic; _intent = intent;
            _relevantSchemaService = relevantSchemaService; _aIQueryService = aIQueryService;
            _sql = sqlExecutionService;

        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto request)
        {
            try
            {
                var normalized =_semantic.Normalize(request.Prompt);

                var intent =_intent.Parse(normalized);

                var schema = _relevantSchemaService.Build(intent.Tables);

                var sql = await _aIQueryService.GenerateSqlAsync(schema,normalized);

                if (!_validator.IsSafe(sql))
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Unsafe SQL"
                    });
                }

                var data = await _sql.ExecuteAsync(sql);

                return Ok(new
                {
                    success = true,
                    sql,
                    count = data.Count(),
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new
                    {
                        success = false,
                        error = ex.Message
                    });
            }
        }


        // ✅ SQL AUTO FIXER
        private string FixSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            // Remove extra spaces
            sql = Regex.Replace(sql, @"\s+", " ");

            // Fix double AND
            sql = sql.Replace("AND AND", "AND");

            // Fix WHERE AND
            sql = sql.Replace("WHERE AND", "WHERE");

            // Fix missing WHERE
            sql = Regex.Replace(
                sql,
                @"FROM\s+(\w+)\s+AND",
                "FROM $1 WHERE",
                RegexOptions.IgnoreCase);

            // Fix active/inactive
            sql = Regex.Replace(
                sql,
                @"=\s*'active'",
                "= 1",
                RegexOptions.IgnoreCase);

            sql = Regex.Replace(
                sql,
                @"=\s*'inactive'",
                "= 0",
                RegexOptions.IgnoreCase);

            // Fix LIMIT -> TOP
            if (sql.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
            {
                var limitMatch = Regex.Match(
                    sql,
                    @"LIMIT\s+(\d+)",
                    RegexOptions.IgnoreCase);

                if (limitMatch.Success)
                {
                    var limit = limitMatch.Groups[1].Value;

                    sql = Regex.Replace(
                        sql,
                        @"SELECT",
                        $"SELECT TOP {limit}",
                        RegexOptions.IgnoreCase);

                    sql = Regex.Replace(
                        sql,
                        @"LIMIT\s+\d+",
                        "",
                        RegexOptions.IgnoreCase);
                }
            }

            // Remove semicolon
            sql = sql.Replace(";", "");

            return sql.Trim();
        }
    }
}
