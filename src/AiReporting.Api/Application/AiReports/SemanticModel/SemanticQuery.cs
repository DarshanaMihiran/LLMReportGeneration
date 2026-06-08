namespace AiReporting.Api.Application.AiReports.SemanticModel;

public sealed class SemanticQuery
{
    public string Dataset { get; set; } = string.Empty;
    public List<string> Metrics { get; set; } = [];
    public List<string> Dimensions { get; set; } = [];
    public List<QueryFilter> Filters { get; set; } = [];
    public bool IncludeForecast { get; set; }
    public string? ForecastPeriod { get; set; }
}
