namespace AiAgent.Core.Models;

public sealed record DiffLine
{
    public string Prefix { get; init; } = " ";
    public string Text { get; init; } = string.Empty;
}
