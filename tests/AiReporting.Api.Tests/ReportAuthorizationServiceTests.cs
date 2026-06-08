using AiReporting.Api.Application.AiReports.Authorization;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Tests;

public sealed class ReportAuthorizationServiceTests
{
    [Fact]
    public async Task AuthorizeAsync_RejectsAnonymousUser()
    {
        var service = new ReportAuthorizationService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AuthorizeAsync("", new SemanticQuery()));
    }

    [Fact]
    public async Task AuthorizeAsync_AllowsAuthenticatedUser()
    {
        var service = new ReportAuthorizationService();

        await service.AuthorizeAsync("demo-user", new SemanticQuery());
    }
}
