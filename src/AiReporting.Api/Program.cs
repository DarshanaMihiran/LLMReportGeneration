using AiReporting.Api.Application.AiReports.Authorization;
using AiReporting.Api.Application.AiReports.Execution;
using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.Intent;
using AiReporting.Api.Application.AiReports.Orchestration;
using AiReporting.Api.Application.AiReports.QueryBuilding;
using AiReporting.Api.Application.AiReports.ReportWriting;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Application.AiReports.Validation;
using AiReporting.Api.Infrastructure.Database;
using AiReporting.Api.Infrastructure.Errors;
using AiReporting.Api.Infrastructure.OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<ReportingOptions>(builder.Configuration.GetSection("Reporting"));

builder.Services.AddHttpClient<IOpenAiChatClient, OpenAiHttpClient>();
builder.Services.AddScoped<IReportIntentExtractor, OpenAiReportIntentExtractor>();
builder.Services.AddScoped<ILlmReportWriter, OpenAiReportWriter>();

builder.Services.AddSingleton<ISemanticModelRegistry, SemanticModelRegistry>();
builder.Services.AddSingleton<ReportingDbConnectionFactory>();
builder.Services.AddScoped<ISemanticQueryValidator, SemanticQueryValidator>();
builder.Services.AddScoped<IReportAuthorizationService, ReportAuthorizationService>();
builder.Services.AddScoped<ISqlQueryBuilder, SqlQueryBuilder>();

if (builder.Configuration.GetValue<bool>("Reporting:UseSampleData"))
{
    builder.Services.AddScoped<IReportingQueryExecutor, SampleReportingQueryExecutor>();
}
else
{
    builder.Services.AddScoped<IReportingQueryExecutor, ReportingQueryExecutor>();
}

builder.Services.AddScoped<IForecastService, OpenAiForecastService>();
builder.Services.AddScoped<IAiReportOrchestrator, AiReportOrchestrator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
