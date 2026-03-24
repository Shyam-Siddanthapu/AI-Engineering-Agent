namespace AiAgent.Core.Models;

public sealed record ProblemRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ProblemType Type { get; init; } = ProblemType.Incident;
    public string? RepositoryUrl { get; init; }
    public string? RepositoryProvider { get; init; }
    public string? Branch { get; init; }
    public string? LogsPath { get; init; }
    public string? ConfigPath { get; init; }
    public bool ApplyChanges { get; init; }
}
