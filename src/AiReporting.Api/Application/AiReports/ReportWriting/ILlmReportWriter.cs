using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.ReportWriting;

public interface ILlmReportWriter
{
    Task<string> WriteAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, SalesForecast? forecast, CancellationToken cancellationToken = default);
}
