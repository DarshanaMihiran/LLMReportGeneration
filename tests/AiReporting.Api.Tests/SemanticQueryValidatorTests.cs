using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Application.AiReports.Validation;

namespace AiReporting.Api.Tests;

public sealed class SemanticQueryValidatorTests
{
    private readonly SemanticQueryValidator _validator = new(new SemanticModelRegistry());

    [Fact]
    public void Validate_AcceptsSupportedSalesSummaryQuery()
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

        _validator.Validate(query);
    }

    [Theory]
    [InlineData("next year")]
    [InlineData("2026")]
    [InlineData("26")]
    [InlineData("2 years")]
    public void Validate_AcceptsUserFriendlyForecastPeriods(string forecastPeriod)
    {
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["minor_sales"],
            Dimensions = ["month"],
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }],
            IncludeForecast = true,
            ForecastPeriod = forecastPeriod
        };

        _validator.Validate(query);
    }

    [Fact]
    public void Validate_RejectsForecastYearBeforeSourceYear()
    {
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["minor_sales"],
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }],
            IncludeForecast = true,
            ForecastPeriod = "2024"
        };

        var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(query));
        Assert.Contains("Unsupported forecast period", exception.Message);
    }

    [Fact]
    public void Validate_RejectsUnsupportedMetric()
    {
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["profit_margin"]
        };

        var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(query));
        Assert.Contains("Unsupported metric", exception.Message);
    }

    [Fact]
    public void Validate_RejectsUnsupportedOperator()
    {
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["minor_sales"],
            Filters = [new QueryFilter { Field = "year", Operator = "LIKE", Value = "2025" }]
        };

        var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(query));
        Assert.Contains("Unsupported operator", exception.Message);
    }
}
