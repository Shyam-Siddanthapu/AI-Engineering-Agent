namespace AiAgent.Core.Models;

public sealed record LogSnapshot(string? Content)
{
    public bool HasContent => !string.IsNullOrWhiteSpace(Content);
}
