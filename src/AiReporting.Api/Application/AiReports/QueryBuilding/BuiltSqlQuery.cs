using Dapper;

namespace AiReporting.Api.Application.AiReports.QueryBuilding;

public sealed class BuiltSqlQuery
{
    public string Sql { get; init; } = string.Empty;
    public DynamicParameters Parameters { get; init; } = new();
}
