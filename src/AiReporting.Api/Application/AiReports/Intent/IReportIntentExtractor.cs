using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Intent;

public interface IReportIntentExtractor
{
    Task<SemanticQuery> ExtractAsync(string prompt, CancellationToken cancellationToken = default);
}
