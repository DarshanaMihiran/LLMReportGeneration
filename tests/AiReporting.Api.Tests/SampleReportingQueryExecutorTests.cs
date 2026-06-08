using AiReporting.Api.Application.AiReports.Execution;
using AiReporting.Api.Application.AiReports.QueryBuilding;

namespace AiReporting.Api.Tests;

public sealed class SampleReportingQueryExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSampleRowsForLocalDevelopment()
    {
        var executor = new SampleReportingQueryExecutor();

        var rows = await executor.ExecuteAsync(new BuiltSqlQuery { Sql = "SELECT TOP 500 1" });

        Assert.Equal(12, rows.Count);
        Assert.True(rows[0].ContainsKey("month"));
        Assert.True(rows[0].ContainsKey("minor_sales"));
    }
}
