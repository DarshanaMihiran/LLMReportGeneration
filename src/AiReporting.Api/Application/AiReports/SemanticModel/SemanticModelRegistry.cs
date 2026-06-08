namespace AiReporting.Api.Application.AiReports.SemanticModel;

public sealed class SemanticModelRegistry : ISemanticModelRegistry
{
    private readonly Dictionary<string, SemanticDataset> _datasets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sales_summary"] = new SemanticDataset
        {
            Name = "sales_summary",
            Description = "Monthly and categorized sales analytics.",
            ViewName = "vw_sales_summary",
            Metrics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["total_sales"] = "TotalSales",
                ["minor_sales"] = "MinorSales",
                ["order_count"] = "OrderCount"
            },
            Dimensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["year"] = "Year",
                ["month"] = "Month",
                ["region"] = "Region",
                ["category"] = "Category"
            },
            Filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["year"] = "Year",
                ["month"] = "Month",
                ["region"] = "Region",
                ["category"] = "Category"
            }
        }
    };

    public IReadOnlyList<SemanticDataset> GetAllDatasets() => _datasets.Values.ToList();

    public SemanticDataset GetDataset(string name)
    {
        if (!_datasets.TryGetValue(name, out var dataset))
        {
            throw new InvalidOperationException($"Unsupported dataset: {name}");
        }

        return dataset;
    }
}
