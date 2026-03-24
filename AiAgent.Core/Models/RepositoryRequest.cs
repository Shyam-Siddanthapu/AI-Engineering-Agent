namespace AiAgent.Core.Models;

public sealed record RepositoryRequest
{
    public string RepositoryUrl { get; init; } = string.Empty;
    public string? RepositoryProvider { get; init; }
    public string? Branch { get; init; }
    public string? Token { get; init; }
    public string? Organization { get; init; }
    public string? Project { get; init; }
    public string? RepositoryName { get; init; }
}
