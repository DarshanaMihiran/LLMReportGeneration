using AiReporting.Api.Application.AiReports.Orchestration;
using AiReporting.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AiReporting.Api.Controllers;

[ApiController]
[Route("api/ai-reports")]
public sealed class AiReportsController : ControllerBase
{
    private readonly IAiReportOrchestrator _orchestrator;

    public AiReportsController(IAiReportOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] AiReportRequest request, CancellationToken cancellationToken)
    {
        var userId = User?.Identity?.Name ?? "demo-user";
        var response = await _orchestrator.GenerateAsync(request, userId, cancellationToken);
        return Ok(response);
    }
}
