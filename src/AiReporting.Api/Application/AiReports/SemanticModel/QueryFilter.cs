namespace AiReporting.Api.Application.AiReports.SemanticModel;

public sealed class QueryFilter
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
