namespace AiAgent.Core.Models;

public sealed record CodeChange
{
    public string FilePath { get; init; } = string.Empty;
    public string OriginalCode { get; init; } = string.Empty;
    public string ModifiedCode { get; init; } = string.Empty;
}
