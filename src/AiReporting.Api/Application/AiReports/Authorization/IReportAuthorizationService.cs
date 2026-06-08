using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Authorization;

public interface IReportAuthorizationService
{
    Task AuthorizeAsync(string userId, SemanticQuery query, CancellationToken cancellationToken = default);
}
