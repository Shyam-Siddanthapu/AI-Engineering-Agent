using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class LlmRequestContext : ILLMRequestContext
{
    private static readonly AsyncLocal<LlmRequestOptions?> CurrentOptions = new();

    public LlmRequestOptions Current => CurrentOptions.Value ?? new LlmRequestOptions();

    public void Set(LlmRequestOptions options)
    {
        CurrentOptions.Value = options;
    }

    public void Clear()
    {
        CurrentOptions.Value = null;
    }
}
