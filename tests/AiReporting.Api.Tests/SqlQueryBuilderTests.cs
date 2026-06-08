using AiReporting.Api.Application.AiReports.QueryBuilding;
using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Tests;

public sealed class SqlQueryBuilderTests
{
    [Fact]
    public void Build_GeneratesParameterizedQueryAgainstMappedView()
    {
        var builder = new SqlQueryBuilder(new SemanticModelRegistry());
        var query = new SemanticQuery
        {
            Dataset = "sales_summary",
            Metrics = ["minor_sales"],
            Dimensions = ["month"],
            Filters = [new QueryFilter { Field = "year", Operator = "=", Value = "2025" }]
        };

        var result = builder.Build(query);

        Assert.Contains("SELECT TOP 500", result.Sql);
        Assert.Contains("[Month] AS [month]", result.Sql);
        Assert.Contains("SUM([MinorSales]) AS [minor_sales]", result.Sql);
        Assert.Contains("FROM vw_sales_summary", result.Sql);
        Assert.Contains("WHERE [Year] = @p0", result.Sql);
        Assert.Contains("GROUP BY [Month]", result.Sql);
        Assert.Contains("ORDER BY [Month]", result.Sql);
        Assert.Contains("p0", result.Parameters.ParameterNames);
        Assert.DoesNotContain("2025", result.Sql);
    }
}
