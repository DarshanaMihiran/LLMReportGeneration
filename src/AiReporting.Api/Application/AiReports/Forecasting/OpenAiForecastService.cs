using System.Text.Json;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Infrastructure.OpenAI;

namespace AiReporting.Api.Application.AiReports.Forecasting;

public sealed class OpenAiForecastService : IForecastService
{
    private readonly IOpenAiChatClient _openAiClient;

    public OpenAiForecastService(IOpenAiChatClient openAiClient)
    {
        _openAiClient = openAiClient;
    }

    public async Task<SalesForecast?> ForecastAsync(
        SemanticQuery query,
        IReadOnlyList<IDictionary<string, object?>> data,
        CancellationToken cancellationToken = default)
    {
        if (!query.IncludeForecast)
        {
            return null;
        }

        var metric = query.Metrics.FirstOrDefault();
        if (metric is null || data.Count == 0)
        {
            return null;
        }

        var historicalTotal = data.Sum(row => TryGetDecimal(row, metric));
        var sourceYear = ForecastPeriodResolver.GetSourceYear(query);
        if (!ForecastPeriodResolver.TryResolveForecastYear(query, out var forecastYear))
        {
            throw new InvalidOperationException($"Unsupported forecast period: {query.ForecastPeriod}");
        }

        var payload = new
        {
            query,
            metric,
            sourceYear,
            forecastYear,
            historicalTotal,
            rows = data
        };
        var request = new
        {
            model = _openAiClient.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You generate conservative business forecasts from verified report data. Use only the supplied rows and totals. Do not invent source data. Return only the requested JSON forecast. The growthRate must be the decimal growth rate implied by predictedTotal versus historicalTotal."
                },
                new
                {
                    role = "user",
                    content = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "sales_forecast",
                    strict = true,
                    schema = BuildSchema()
                }
            }
        };

        using var response = await _openAiClient.PostChatCompletionAsync(request, cancellationToken);
        var content = response.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI returned an empty forecast.");
        }

        var forecast = JsonSerializer.Deserialize<SalesForecast>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ??
            throw new InvalidOperationException("OpenAI returned an invalid forecast.");

        ValidateForecast(forecast, forecastYear, historicalTotal);
        forecast.Method = string.IsNullOrWhiteSpace(forecast.Method)
            ? "OpenAI structured forecast from verified report data"
            : forecast.Method;

        return forecast;
    }

    private static object BuildSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            forecastYear = new { type = "integer" },
            predictedTotal = new { type = "number" },
            growthRate = new { type = "number" },
            method = new { type = "string" }
        },
        required = new[] { "forecastYear", "predictedTotal", "growthRate", "method" }
    };

    private static void ValidateForecast(SalesForecast forecast, int expectedForecastYear, decimal historicalTotal)
    {
        if (forecast.ForecastYear != expectedForecastYear)
        {
            throw new InvalidOperationException($"OpenAI returned forecast year {forecast.ForecastYear}, expected {expectedForecastYear}.");
        }

        if (forecast.PredictedTotal < 0)
        {
            throw new InvalidOperationException("OpenAI returned a negative forecast total.");
        }

        if (historicalTotal > 0)
        {
            var impliedGrowthRate = Math.Round((forecast.PredictedTotal - historicalTotal) / historicalTotal, 4);
            var returnedGrowthRate = Math.Round(forecast.GrowthRate, 4);

            if (Math.Abs(impliedGrowthRate - returnedGrowthRate) > 0.01m)
            {
                throw new InvalidOperationException("OpenAI returned a growth rate that does not match the predicted total.");
            }
        }
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
