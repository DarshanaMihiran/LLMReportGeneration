using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Authorization;

public sealed class ReportAuthorizationService : IReportAuthorizationService
{
    public Task AuthorizeAsync(string userId, SemanticQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return Task.CompletedTask;
    }
}
