using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Services;

namespace AiAgent.Infrastructure.Llm;

public sealed class OllamaChatCompletionService : IChatCompletionService
{
    private readonly OllamaClient _client;

    public OllamaChatCompletionService(OllamaClient client)
    {
        _client = client;
    }

    public Task<string> GetCompletionAsync(string prompt, string model, string apiKey, CancellationToken cancellationToken = default)
    {
        var options = new LlmRequestOptions
        {
            Provider = LlmProvider.Ollama,
            Model = string.IsNullOrWhiteSpace(model) ? null : model
        };

        return _client.GenerateAsync(prompt, options, cancellationToken);
    }
}
