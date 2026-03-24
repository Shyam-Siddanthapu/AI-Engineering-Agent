namespace AiAgent.Core.Models;

public sealed record ConfigSnapshot(string? Content)
{
    public bool HasContent => !string.IsNullOrWhiteSpace(Content);
}
