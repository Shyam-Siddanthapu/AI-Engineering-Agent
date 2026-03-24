using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Services;

namespace AiAgent.Infrastructure.Llm;

public sealed class AzureOpenAIChatCompletionService : IChatCompletionService
{
    private readonly AzureOpenAiClient _client;

    public AzureOpenAIChatCompletionService(AzureOpenAiClient client)
    {
        _client = client;
    }

    public Task<string> GetCompletionAsync(string prompt, string model, string apiKey, CancellationToken cancellationToken = default)
    {
        var options = new LlmRequestOptions
        {
            Provider = LlmProvider.AzureOpenAI,
            Model = string.IsNullOrWhiteSpace(model) ? null : model,
            ApiKey = apiKey
        };

        return _client.GenerateAsync(prompt, options, cancellationToken);
    }
}
