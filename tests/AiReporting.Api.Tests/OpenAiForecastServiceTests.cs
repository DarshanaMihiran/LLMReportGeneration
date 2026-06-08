using System.Text.Json;
using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Infrastructure.OpenAI;

namespace AiReporting.Api.Tests;

public sealed class OpenAiForecastServiceTests
{
    [Fact]
    public async Task ForecastAsync_UsesStructuredOpenAiForecast()
    {
        var openAiClient = new FakeOpenAiChatClient("""
        {
          "choices": [
            {
              "message": {
                "content": "{\"forecastYear\":2026,\"predictedTotal\":324,\"growthRate\":0.08,\"method\":\"OpenAI trend forecast\"}"
              }
            }
          ]
        }
        """);
        var service = new OpenAiForecastService(openAiClient);
        var query = new SemanticQuery
        {
            Metrics = ["minor_sales"],
            IncludeForecast = true,
            ForecastPeriod = "next_year",
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }]
        };
        IReadOnlyList<IDictionary<string, object?>> data =
        [
            new Dictionary<string, object?> { ["minor_sales"] = 100m },
            new Dictionary<string, object?> { ["minor_sales"] = 200m }
        ];

        var forecast = await service.ForecastAsync(query, data);

        Assert.NotNull(forecast);
        Assert.Equal(2026, forecast.ForecastYear);
        Assert.Equal(324m, forecast.PredictedTotal);
        Assert.Equal(0.08m, forecast.GrowthRate);
        Assert.Equal("OpenAI trend forecast", forecast.Method);
    }

    [Fact]
    public async Task ForecastAsync_UsesExplicitForecastYearFromForecastPeriod()
    {
        var openAiClient = new FakeOpenAiChatClient("""
        {
          "choices": [
            {
              "message": {
                "content": "{\"forecastYear\":2027,\"predictedTotal\":330,\"growthRate\":0.1,\"method\":\"OpenAI explicit year forecast\"}"
              }
            }
          ]
        }
        """);
        var service = new OpenAiForecastService(openAiClient);
        var query = new SemanticQuery
        {
            Metrics = ["minor_sales"],
            IncludeForecast = true,
            ForecastPeriod = "2027",
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }]
        };
        IReadOnlyList<IDictionary<string, object?>> data =
        [
            new Dictionary<string, object?> { ["minor_sales"] = 100m },
            new Dictionary<string, object?> { ["minor_sales"] = 200m }
        ];

        var forecast = await service.ForecastAsync(query, data);

        Assert.NotNull(forecast);
        Assert.Equal(2027, forecast.ForecastYear);
    }

    [Fact]
    public async Task ForecastAsync_RejectsInconsistentGrowthRate()
    {
        var openAiClient = new FakeOpenAiChatClient("""
        {
          "choices": [
            {
              "message": {
                "content": "{\"forecastYear\":2026,\"predictedTotal\":324,\"growthRate\":0.25,\"method\":\"Invalid forecast\"}"
              }
            }
          ]
        }
        """);
        var service = new OpenAiForecastService(openAiClient);
        var query = new SemanticQuery
        {
            Metrics = ["minor_sales"],
            IncludeForecast = true,
            ForecastPeriod = "next_year",
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }]
        };
        IReadOnlyList<IDictionary<string, object?>> data =
        [
            new Dictionary<string, object?> { ["minor_sales"] = 100m },
            new Dictionary<string, object?> { ["minor_sales"] = 200m }
        ];

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ForecastAsync(query, data));
    }

    private sealed class FakeOpenAiChatClient : IOpenAiChatClient
    {
        private readonly string _json;

        public FakeOpenAiChatClient(string json)
        {
            _json = json;
        }

        public string Model => "test-model";

        public Task<JsonDocument> PostChatCompletionAsync(object request, CancellationToken cancellationToken)
        {
            return Task.FromResult(JsonDocument.Parse(_json));
        }
    }
}
