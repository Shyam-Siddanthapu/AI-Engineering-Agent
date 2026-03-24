namespace AiAgent.Core.Models;

public sealed record AgentRequest
{
    public string RepoUrl { get; init; } = string.Empty;
    public string Task { get; init; } = string.Empty;
    public string Provider { get; init; } = "Ollama";
    public string? Model { get; init; }
    public string? ApiKey { get; init; }
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.Preview;
}
