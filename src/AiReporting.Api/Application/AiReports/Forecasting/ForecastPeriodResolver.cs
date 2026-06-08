using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Forecasting;

public static class ForecastPeriodResolver
{
    private const int MaxForecastHorizonYears = 10;

    public static int GetSourceYear(SemanticQuery query)
    {
        return query.Filters
            .Where(filter => string.Equals(filter.Field, "year", StringComparison.OrdinalIgnoreCase) && int.TryParse(filter.Value, out _))
            .Select(filter => int.Parse(filter.Value))
            .DefaultIfEmpty(DateTime.UtcNow.Year)
            .Max();
    }

    public static bool TryResolveForecastYear(SemanticQuery query, out int forecastYear)
    {
        var sourceYear = GetSourceYear(query);
        return TryResolveForecastYear(query.ForecastPeriod, sourceYear, out forecastYear);
    }

    public static bool TryResolveForecastYear(string? forecastPeriod, int sourceYear, out int forecastYear)
    {
        forecastYear = sourceYear + 1;

        if (string.IsNullOrWhiteSpace(forecastPeriod))
        {
            return false;
        }

        var normalized = forecastPeriod.Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");

        if (normalized is "next year" or "nextyear")
        {
            forecastYear = sourceYear + 1;
            return true;
        }

        if (normalized.EndsWith(" year") && int.TryParse(normalized[..^5].Trim(), out var yearsAhead))
        {
            forecastYear = sourceYear + yearsAhead;
            return IsSupportedForecastYear(sourceYear, forecastYear);
        }

        if (normalized.EndsWith(" years") && int.TryParse(normalized[..^6].Trim(), out yearsAhead))
        {
            forecastYear = sourceYear + yearsAhead;
            return IsSupportedForecastYear(sourceYear, forecastYear);
        }

        if (normalized.StartsWith("next ") && int.TryParse(normalized[5..].Trim().Split(' ')[0], out yearsAhead))
        {
            forecastYear = sourceYear + yearsAhead;
            return IsSupportedForecastYear(sourceYear, forecastYear);
        }

        if (int.TryParse(normalized, out var explicitYear))
        {
            if (explicitYear is >= 0 and <= 99)
            {
                explicitYear += sourceYear / 100 * 100;
            }

            forecastYear = explicitYear;
            return IsSupportedForecastYear(sourceYear, forecastYear);
        }

        return false;
    }

    private static bool IsSupportedForecastYear(int sourceYear, int forecastYear)
    {
        return forecastYear > sourceYear && forecastYear <= sourceYear + MaxForecastHorizonYears;
    }
}
