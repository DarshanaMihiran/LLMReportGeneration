using AiReporting.Api.Application.AiReports.QueryBuilding;
using AiReporting.Api.Infrastructure.Database;
using Dapper;

namespace AiReporting.Api.Application.AiReports.Execution;

public sealed class ReportingQueryExecutor : IReportingQueryExecutor
{
    private readonly ReportingDbConnectionFactory _connectionFactory;

    public ReportingQueryExecutor(ReportingDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteAsync(BuiltSqlQuery query, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(query.Sql, query.Parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync(command);

        return rows
            .Select(row => new Dictionary<string, object?>((IDictionary<string, object?>)row, StringComparer.OrdinalIgnoreCase))
            .Cast<IDictionary<string, object?>>()
            .ToList();
    }
}
