using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ILLMRequestContext
{
    LlmRequestOptions Current { get; }
    void Set(LlmRequestOptions options);
    void Clear();
}
