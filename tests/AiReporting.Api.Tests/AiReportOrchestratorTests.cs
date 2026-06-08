using AiReporting.Api.Application.AiReports.Authorization;
using AiReporting.Api.Application.AiReports.Execution;
using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.Intent;
using AiReporting.Api.Application.AiReports.Orchestration;
using AiReporting.Api.Application.AiReports.QueryBuilding;
using AiReporting.Api.Application.AiReports.ReportWriting;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Application.AiReports.Validation;
using AiReporting.Api.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiReporting.Api.Tests;

public sealed class AiReportOrchestratorTests
{
    [Fact]
    public async Task GenerateAsync_RunsFullPipeline()
    {
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["minor_sales"],
            Dimensions = ["month"],
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }],
            IncludeForecast = true,
            ForecastPeriod = "next_year"
        };
        var data = new List<IDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["month"] = 1, ["minor_sales"] = 100m }
        };
        var forecast = new SalesForecast { ForecastYear = 2026, PredictedTotal = 108m, GrowthRate = 0.08m };
        var orchestrator = new AiReportOrchestrator(
            new FakeIntentExtractor(query),
            new SemanticQueryValidator(new SemanticModelRegistry()),
            new ReportAuthorizationService(),
            new FakeSqlQueryBuilder(),
            new FakeQueryExecutor(data),
            new FakeForecastService(forecast),
            new FakeReportWriter("report text"),
            NullLogger<AiReportOrchestrator>.Instance);

        var response = await orchestrator.GenerateAsync(new AiReportRequest { Prompt = "monthly minor sales previous year" }, "demo-user");

        Assert.Same(query, response.Query);
        Assert.Same(data, response.Data);
        Assert.Same(forecast, response.Forecast);
        Assert.Equal("report text", response.Report);
    }

    [Fact]
    public async Task GenerateAsync_RejectsEmptyPrompt()
    {
        var orchestrator = new AiReportOrchestrator(
            new FakeIntentExtractor(new SemanticQuery()),
            new SemanticQueryValidator(new SemanticModelRegistry()),
            new ReportAuthorizationService(),
            new FakeSqlQueryBuilder(),
            new FakeQueryExecutor([]),
            new FakeForecastService(null),
            new FakeReportWriter(""),
            NullLogger<AiReportOrchestrator>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => orchestrator.GenerateAsync(new AiReportRequest(), "demo-user"));
    }

    private sealed class FakeIntentExtractor : IReportIntentExtractor
    {
        private readonly SemanticQuery _query;

        public FakeIntentExtractor(SemanticQuery query) => _query = query;

        public Task<SemanticQuery> ExtractAsync(string prompt, CancellationToken cancellationToken = default) => Task.FromResult(_query);
    }

    private sealed class FakeSqlQueryBuilder : ISqlQueryBuilder
    {
        public BuiltSqlQuery Build(SemanticQuery query) => new() { Sql = "SELECT TOP 500 1" };
    }

    private sealed class FakeQueryExecutor : IReportingQueryExecutor
    {
        private readonly IReadOnlyList<IDictionary<string, object?>> _data;

        public FakeQueryExecutor(IReadOnlyList<IDictionary<string, object?>> data) => _data = data;

        public Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteAsync(BuiltSqlQuery query, CancellationToken cancellationToken = default) => Task.FromResult(_data);
    }

    private sealed class FakeForecastService : IForecastService
    {
        private readonly SalesForecast? _forecast;

        public FakeForecastService(SalesForecast? forecast) => _forecast = forecast;

        public Task<SalesForecast?> ForecastAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, CancellationToken cancellationToken = default) => Task.FromResult(_forecast);
    }

    private sealed class FakeReportWriter : ILlmReportWriter
    {
        private readonly string _report;

        public FakeReportWriter(string report) => _report = report;

        public Task<string> WriteAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, SalesForecast? forecast, CancellationToken cancellationToken = default) => Task.FromResult(_report);
    }
}
