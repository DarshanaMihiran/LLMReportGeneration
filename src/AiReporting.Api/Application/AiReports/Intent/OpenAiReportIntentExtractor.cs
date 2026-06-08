using System.Text.Json;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Infrastructure.OpenAI;

namespace AiReporting.Api.Application.AiReports.Intent;

public sealed class OpenAiReportIntentExtractor : IReportIntentExtractor
{
    private readonly IOpenAiChatClient _openAiClient;
    private readonly ISemanticModelRegistry _registry;

    public OpenAiReportIntentExtractor(IOpenAiChatClient openAiClient, ISemanticModelRegistry registry)
    {
        _openAiClient = openAiClient;
        _registry = registry;
    }

    public async Task<SemanticQuery> ExtractAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _openAiClient.Model,
            messages = new object[]
            {
                new { role = "system", content = BuildSystemPrompt() },
                new { role = "user", content = prompt }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "semantic_report_query",
                    strict = true,
                    schema = BuildSchema()
                }
            }
        };

        using var response = await _openAiClient.PostChatCompletionAsync(request, cancellationToken);
        var content = response.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI returned an empty semantic query.");
        }

        return JsonSerializer.Deserialize<SemanticQuery>(content, JsonOptions()) ??
            throw new InvalidOperationException("OpenAI returned an invalid semantic query.");
    }

    private string BuildSystemPrompt()
    {
        var datasets = _registry.GetAllDatasets().Select(dataset => new
        {
            dataset.Name,
            dataset.Description,
            metrics = dataset.Metrics.Keys,
            dimensions = dataset.Dimensions.Keys,
            filters = dataset.Filters.Keys
        });

        return "You convert user reporting requests into semantic query JSON.\n" +
               "Rules:\n" +
               "- Do not generate SQL.\n" +
               "- Use only the provided datasets, metrics, dimensions, and filters.\n" +
               "- Use previous year as a year filter when user says \"previous year\".\n" +
               "- Use month as a dimension when user asks for monthly reports.\n" +
               "- If user asks forecast, prediction, or next year estimate, set includeForecast to true.\n" +
               "- For forecastPeriod, use user-friendly values such as \"next year\" or an explicit year like \"2026\".\n" +
               "- If the user asks for next N years, set forecastPeriod to \"N years\".\n" +
               "- If the request cannot be represented using the available semantic metadata, choose the closest valid query and avoid inventing unsupported fields.\n" +
               $"Semantic metadata: {JsonSerializer.Serialize(datasets)}";
    }

    private static object BuildSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            dataset = new { type = "string", @enum = new[] { "sales_summary" } },
            metrics = new { type = "array", items = new { type = "string", @enum = new[] { "total_sales", "minor_sales", "order_count" } } },
            dimensions = new { type = "array", items = new { type = "string", @enum = new[] { "year", "month", "region", "category" } } },
            filters = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        field = new { type = "string", @enum = new[] { "year", "month", "region", "category" } },
                        @operator = new { type = "string", @enum = new[] { "=", ">=", "<=", ">", "<" } },
                        value = new { type = "string" }
                    },
                    required = new[] { "field", "operator", "value" }
                }
            },
            includeForecast = new { type = "boolean" },
            forecastPeriod = new { type = new object[] { "string", "null" } }
        },
        required = new[] { "dataset", "metrics", "dimensions", "filters", "includeForecast", "forecastPeriod" }
    };

    private static JsonSerializerOptions JsonOptions() => new(JsonSerializerDefaults.Web);
}
