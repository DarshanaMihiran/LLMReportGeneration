using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Validation;

public sealed class SemanticQueryValidator : ISemanticQueryValidator
{
    private static readonly HashSet<string> AllowedOperators = new(StringComparer.OrdinalIgnoreCase) { "=", ">=", "<=", ">", "<" };
    private readonly ISemanticModelRegistry _registry;

    public SemanticQueryValidator(ISemanticModelRegistry registry)
    {
        _registry = registry;
    }

    public void Validate(SemanticQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Dataset))
        {
            throw new ArgumentException("Dataset is required.");
        }

        var dataset = _registry.GetDataset(query.Dataset);

        if (query.Metrics.Count == 0)
        {
            throw new ArgumentException("At least one metric is required.");
        }

        if (query.Dimensions.Count > 5)
        {
            throw new ArgumentException("A maximum of 5 dimensions is allowed.");
        }

        if (query.Filters.Count > 10)
        {
            throw new ArgumentException("A maximum of 10 filters is allowed.");
        }

        foreach (var metric in query.Metrics)
        {
            if (!dataset.Metrics.ContainsKey(metric))
            {
                throw new ArgumentException($"Unsupported metric: {metric}");
            }
        }

        foreach (var dimension in query.Dimensions)
        {
            if (!dataset.Dimensions.ContainsKey(dimension))
            {
                throw new ArgumentException($"Unsupported dimension: {dimension}");
            }
        }

        foreach (var filter in query.Filters)
        {
            if (!dataset.Filters.ContainsKey(filter.Field))
            {
                throw new ArgumentException($"Unsupported filter: {filter.Field}");
            }

            if (!AllowedOperators.Contains(filter.Operator))
            {
                throw new ArgumentException($"Unsupported operator: {filter.Operator}");
            }

            if (string.IsNullOrWhiteSpace(filter.Value))
            {
                throw new ArgumentException($"Filter value is required for {filter.Field}.");
            }
        }

        if (query.IncludeForecast && !ForecastPeriodResolver.TryResolveForecastYear(query, out _))
        {
            throw new ArgumentException($"Unsupported forecast period: {query.ForecastPeriod}");
        }
    }
}
