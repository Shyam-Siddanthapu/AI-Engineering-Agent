namespace AiAgent.Core.Models;

public sealed record LlmRequestOptions
{
    public LlmProvider Provider { get; init; } = LlmProvider.Ollama;
    public string? Model { get; init; }
    public string? ApiKey { get; init; }
}
