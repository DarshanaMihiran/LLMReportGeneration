using AiReporting.Api.Application.AiReports.QueryBuilding;

namespace AiReporting.Api.Application.AiReports.Execution;

public sealed class SampleReportingQueryExecutor : IReportingQueryExecutor
{
    public Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteAsync(BuiltSqlQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<IDictionary<string, object?>> rows =
        [
            Row(2025, 1, "North", "Retail", 125000m, 42000m, 118),
            Row(2025, 2, "North", "Retail", 132500m, 46100m, 126),
            Row(2025, 3, "North", "Retail", 141200m, 48950m, 134),
            Row(2025, 4, "North", "Retail", 139800m, 47200m, 129),
            Row(2025, 5, "North", "Retail", 151400m, 51300m, 141),
            Row(2025, 6, "North", "Retail", 158900m, 54800m, 149),
            Row(2025, 7, "North", "Retail", 162300m, 56250m, 153),
            Row(2025, 8, "North", "Retail", 168700m, 59100m, 160),
            Row(2025, 9, "North", "Retail", 171500m, 60300m, 164),
            Row(2025, 10, "North", "Retail", 176800m, 62950m, 171),
            Row(2025, 11, "North", "Retail", 184600m, 65800m, 179),
            Row(2025, 12, "North", "Retail", 196200m, 70400m, 193)
        ];

        return Task.FromResult(rows);
    }

    private static IDictionary<string, object?> Row(
        int year,
        int month,
        string region,
        string category,
        decimal totalSales,
        decimal minorSales,
        int orderCount)
    {
        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["year"] = year,
            ["month"] = month,
            ["region"] = region,
            ["category"] = category,
            ["total_sales"] = totalSales,
            ["minor_sales"] = minorSales,
            ["order_count"] = orderCount
        };
    }
}
