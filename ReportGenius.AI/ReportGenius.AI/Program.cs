using ReportGenius.AI.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region :: ::
builder.Services.AddHttpClient<GroqService>();

builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<SqlSafetyService>();
builder.Services.AddScoped<SchemaService>();
builder.Services.AddScoped<SqlValidatorService>();
builder.Services.AddScoped<ColumnMapperService>();

builder.Services.AddScoped<TableMapperService>();
builder.Services.AddScoped<PromptNormalizerService>();
builder.Services.AddScoped<SqlJoinFixService>();
builder.Services.AddScoped<SqlSyntaxFixService>();
builder.Services.AddScoped<SqlTableValidatorService>();
builder.Services.AddScoped<DynamicSqlFixService>();
builder.Services.AddHttpClient<GemmaService>();
//-----

builder.Services.AddSingleton<MetadataCacheService>();

builder.Services.AddScoped<SemanticMapperService>();

builder.Services.AddScoped<PromptUnderstandingService>();

builder.Services.AddScoped<RelevantSchemaService>();

builder.Services.AddScoped<SqlExecutionService>();

builder.Services.AddHttpClient<AIQueryService>();


#endregion

var app = builder.Build();
var cache = app.Services.GetRequiredService<MetadataCacheService>();
await cache.LoadAsync();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
