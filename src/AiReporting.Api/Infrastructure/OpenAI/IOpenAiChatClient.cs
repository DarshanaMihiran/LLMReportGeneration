using System.Text.Json;

namespace AiReporting.Api.Infrastructure.OpenAI;

public interface IOpenAiChatClient
{
    string Model { get; }

    Task<JsonDocument> PostChatCompletionAsync(object request, CancellationToken cancellationToken);
}
