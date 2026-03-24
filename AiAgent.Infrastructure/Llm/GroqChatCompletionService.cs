using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Services;

namespace AiAgent.Infrastructure.Llm;

public sealed class GroqChatCompletionService : IChatCompletionService
{
    private readonly GroqClient _client;

    public GroqChatCompletionService(GroqClient client)
    {
        _client = client;
    }

    public Task<string> GetCompletionAsync(string prompt, string model, string apiKey, CancellationToken cancellationToken = default)
    {
        var options = new LlmRequestOptions
        {
            Provider = LlmProvider.Groq,
            Model = string.IsNullOrWhiteSpace(model) ? null : model,
            ApiKey = apiKey
        };

        return _client.GenerateAsync(prompt, options, cancellationToken);
    }
}
