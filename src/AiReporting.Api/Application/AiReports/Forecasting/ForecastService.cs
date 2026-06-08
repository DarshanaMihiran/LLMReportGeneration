using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Forecasting;

public sealed class ForecastService : IForecastService
{
    private const decimal GrowthRate = 0.08m;

    public Task<SalesForecast?> ForecastAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, CancellationToken cancellationToken = default)
    {
        if (!query.IncludeForecast)
        {
            return Task.FromResult<SalesForecast?>(null);
        }

        var metric = query.Metrics.FirstOrDefault();
        if (metric is null)
        {
            return Task.FromResult<SalesForecast?>(null);
        }

        var total = data.Sum(row => TryGetDecimal(row, metric));
        if (!ForecastPeriodResolver.TryResolveForecastYear(query, out var forecastYear))
        {
            throw new InvalidOperationException($"Unsupported forecast period: {query.ForecastPeriod}");
        }

        return Task.FromResult<SalesForecast?>(new SalesForecast
        {
            ForecastYear = forecastYear,
            PredictedTotal = Math.Round(total * (1 + GrowthRate), 2),
            GrowthRate = GrowthRate,
            Method = "Simple 8% growth assumption MVP forecast"
        });
    }

    private static decimal TryGetDecimal(IDictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value is null)
        {
            return 0m;
        }

        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue),
            float floatValue => Convert.ToDecimal(floatValue),
            int intValue => intValue,
            long longValue => longValue,
            _ when decimal.TryParse(value.ToString(), out var parsed) => parsed,
            _ => 0m
        };
    }
}
