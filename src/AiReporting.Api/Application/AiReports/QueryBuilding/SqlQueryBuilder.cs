using System.Text;
using AiReporting.Api.Application.AiReports.SemanticModel;
using Dapper;

namespace AiReporting.Api.Application.AiReports.QueryBuilding;

public sealed class SqlQueryBuilder : ISqlQueryBuilder
{
    private readonly ISemanticModelRegistry _registry;

    public SqlQueryBuilder(ISemanticModelRegistry registry)
    {
        _registry = registry;
    }

    public BuiltSqlQuery Build(SemanticQuery query)
    {
        var dataset = _registry.GetDataset(query.Dataset);
        var selectParts = new List<string>();
        var groupByParts = new List<string>();
        var orderByParts = new List<string>();

        foreach (var dimension in query.Dimensions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var column = dataset.Dimensions[dimension];
            selectParts.Add($"    [{column}] AS [{dimension}]");
            groupByParts.Add($"[{column}]");
            orderByParts.Add($"[{column}]");
        }

        foreach (var metric in query.Metrics.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var column = dataset.Metrics[metric];
            selectParts.Add($"    SUM([{column}]) AS [{metric}]");
        }

        var parameters = new DynamicParameters();
        var whereParts = new List<string>();

        for (var i = 0; i < query.Filters.Count; i++)
        {
            var filter = query.Filters[i];
            var column = dataset.Filters[filter.Field];
            var parameterName = $"p{i}";
            whereParts.Add($"[{column}] {filter.Operator} @{parameterName}");
            parameters.Add(parameterName, ConvertFilterValue(filter.Value));
        }

        var sql = new StringBuilder();
        sql.AppendLine("SELECT TOP 500");
        sql.AppendLine(string.Join(",\n", selectParts));
        sql.AppendLine($"FROM {dataset.ViewName}");

        if (whereParts.Count > 0)
        {
            sql.AppendLine("WHERE " + string.Join(" AND ", whereParts));
        }

        if (groupByParts.Count > 0)
        {
            sql.AppendLine("GROUP BY " + string.Join(", ", groupByParts));
            sql.AppendLine("ORDER BY " + string.Join(", ", orderByParts));
        }

        return new BuiltSqlQuery
        {
            Sql = sql.ToString().TrimEnd(),
            Parameters = parameters
        };
    }

    private static object ConvertFilterValue(string value)
    {
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        if (decimal.TryParse(value, out var decimalValue))
        {
            return decimalValue;
        }

        return value;
    }
}
