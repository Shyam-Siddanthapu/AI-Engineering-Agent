namespace AiAgent.Core.Models;

public sealed record LogAnalysisResult(
    string RootCause,
    IReadOnlyList<string> AffectedComponents,
    string SuggestedFix);
