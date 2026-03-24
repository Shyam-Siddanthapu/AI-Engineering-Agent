namespace AiAgent.Core.Models;

public sealed record DiffResult
{
    public IReadOnlyList<DiffLine> Lines { get; init; } = Array.Empty<DiffLine>();
}
