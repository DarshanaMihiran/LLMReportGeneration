using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Tests;

public sealed class ForecastServiceTests
{
    [Fact]
    public async Task ForecastAsync_ReturnsNullWhenForecastNotRequested()
    {
        var service = new ForecastService();
        var query = new SemanticQuery { Metrics = ["minor_sales"], IncludeForecast = false };

        var result = await service.ForecastAsync(query, []);

        Assert.Null(result);
    }

    [Fact]
    public async Task ForecastAsync_AppliesEightPercentGrowthToFirstMetric()
    {
        var service = new ForecastService();
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

        var result = await service.ForecastAsync(query, data);

        Assert.NotNull(result);
        Assert.Equal(2026, result.ForecastYear);
        Assert.Equal(324m, result.PredictedTotal);
        Assert.Equal(0.08m, result.GrowthRate);
    }

    [Theory]
    [InlineData("next year", 2026)]
    [InlineData("2026", 2026)]
    [InlineData("26", 2026)]
    [InlineData("2 years", 2027)]
    public async Task ForecastAsync_ResolvesUserFriendlyForecastPeriods(string forecastPeriod, int expectedYear)
    {
        var service = new ForecastService();
        var query = new SemanticQuery
        {
            Metrics = ["minor_sales"],
            IncludeForecast = true,
            ForecastPeriod = forecastPeriod,
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }]
        };
        IReadOnlyList<IDictionary<string, object?>> data =
        [
            new Dictionary<string, object?> { ["minor_sales"] = 100m }
        ];

        var result = await service.ForecastAsync(query, data);

        Assert.NotNull(result);
        Assert.Equal(expectedYear, result.ForecastYear);
    }
}
