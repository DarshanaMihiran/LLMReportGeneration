using Microsoft.Data.SqlClient;

namespace AiReporting.Api.Infrastructure.Database;

public sealed class ReportingDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public ReportingDbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("ReportingDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ReportingDb connection string is not configured.");
        }

        return new SqlConnection(connectionString);
    }
}
