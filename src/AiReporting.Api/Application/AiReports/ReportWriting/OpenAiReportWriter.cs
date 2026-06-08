using System.Text.Json;
using AiReporting.Api.Application.AiReports.Forecasting;
using AiReporting.Api.Application.AiReports.SemanticModel;
using AiReporting.Api.Infrastructure.OpenAI;

namespace AiReporting.Api.Application.AiReports.ReportWriting;

public sealed class OpenAiReportWriter : ILlmReportWriter
{
    private readonly IOpenAiChatClient _openAiClient;

    public OpenAiReportWriter(IOpenAiChatClient openAiClient)
    {
        _openAiClient = openAiClient;
    }

    public async Task<string> WriteAsync(SemanticQuery query, IReadOnlyList<IDictionary<string, object?>> data, SalesForecast? forecast, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new { query, data, forecast }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var request = new
        {
            model = _openAiClient.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are a senior business analyst writing reports from verified data. Use only the supplied data. Do not invent numbers. Clearly mention forecast assumptions. Keep the report concise and professional."
                },
                new { role = "user", content = payload }
            }
        };

        using var response = await _openAiClient.PostChatCompletionAsync(request, cancellationToken);
        return response.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
