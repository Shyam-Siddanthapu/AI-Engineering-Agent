using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class MockLlmClient : ILLMProviderClient
{
    public LlmProvider Provider => LlmProvider.Mock;

    public Task<string> GenerateAsync(string prompt, LlmRequestOptions options, CancellationToken cancellationToken)
    {
        var response = """
        {
          "intent": "Mock",
          "summary": "This is a mock response for testing the AI agent pipeline.",
          "detailedExplanation": "The mock provider simulates structured output without calling external services.",
          "steps": [
            "Receive input",
            "Return deterministic JSON"
          ],
          "codeChanges": [],
          "testCases": [],
          "risks": ["Mock output is not real analysis"],
          "suggestions": ["Use Ollama or Azure OpenAI for real output"]
        }
        """;

        return Task.FromResult(response);
    }
}
