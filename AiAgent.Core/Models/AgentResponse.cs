namespace AiAgent.Core.Models;

public sealed record AgentResponse
{
    public string Intent { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string DetailedExplanation { get; init; } = string.Empty;
    public List<string> Steps { get; init; } = [];
    public List<CodeChange> CodeChanges { get; init; } = [];
    public List<string> TestCases { get; init; } = [];
    public List<string> Risks { get; init; } = [];
    public List<string> Suggestions { get; init; } = [];
}
