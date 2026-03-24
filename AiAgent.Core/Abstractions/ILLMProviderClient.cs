using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ILLMProviderClient
{
    LlmProvider Provider { get; }
    Task<string> GenerateAsync(string prompt, LlmRequestOptions options, CancellationToken cancellationToken);
}
