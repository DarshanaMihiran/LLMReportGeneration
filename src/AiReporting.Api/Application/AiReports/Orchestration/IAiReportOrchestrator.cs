using AiReporting.Api.Contracts;

namespace AiReporting.Api.Application.AiReports.Orchestration;

public interface IAiReportOrchestrator
{
    Task<AiReportResponse> GenerateAsync(AiReportRequest request, string userId, CancellationToken cancellationToken = default);
}
