using AiReporting.Api.Application.AiReports.QueryBuilding;

namespace AiReporting.Api.Application.AiReports.Execution;

public interface IReportingQueryExecutor
{
    Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteAsync(BuiltSqlQuery query, CancellationToken cancellationToken = default);
}
