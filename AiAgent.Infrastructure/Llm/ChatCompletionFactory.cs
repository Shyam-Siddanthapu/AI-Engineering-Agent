using AiAgent.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AiAgent.Infrastructure.Llm;

public sealed class ChatCompletionFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ChatCompletionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IChatCompletionService GetService(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return _serviceProvider.GetRequiredService<OllamaChatCompletionService>();
        }

        return provider.Trim().ToLowerInvariant() switch
        {
            "ollama" => _serviceProvider.GetRequiredService<OllamaChatCompletionService>(),
            "azure" or "azureopenai" or "azure-openai" => _serviceProvider.GetRequiredService<AzureOpenAIChatCompletionService>(),
            "groq" => _serviceProvider.GetRequiredService<GroqChatCompletionService>(),
            "mock" => _serviceProvider.GetRequiredService<MockChatCompletionService>(),
            _ => _serviceProvider.GetRequiredService<OllamaChatCompletionService>()
        };
    }
}
