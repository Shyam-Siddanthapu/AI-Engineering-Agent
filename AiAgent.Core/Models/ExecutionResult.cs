namespace AiAgent.Core.Models;

public sealed record ExecutionResult(
    IReadOnlyList<string> Plan,
    AgentResponse Response);
