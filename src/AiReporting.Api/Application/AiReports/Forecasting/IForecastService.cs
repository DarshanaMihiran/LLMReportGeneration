using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Forecasting;

public interface IForecastService
{
    Task<SalesForecast?> ForecastAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, CancellationToken cancellationToken = default);
}
