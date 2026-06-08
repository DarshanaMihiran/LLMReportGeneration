using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Contracts;

public sealed class AiReportResponse
{
    public SemanticQuery Query { get; set; } = new();
    public IReadOnlyList<IDictionary<string, object?>> Data { get; set; } = [];
    public SalesForecast? Forecast { get; set; }
    public string Report { get; set; } = string.Empty;
}
