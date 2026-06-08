namespace AiReporting.Api.Application.AiReports.SemanticModel;

public sealed class SemanticDataset
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ViewName { get; init; } = string.Empty;
    public Dictionary<string, string> Metrics { get; init; } = [];
    public Dictionary<string, string> Dimensions { get; init; } = [];
    public Dictionary<string, string> Filters { get; init; } = [];
}
