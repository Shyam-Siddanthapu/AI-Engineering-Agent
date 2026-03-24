namespace AiAgent.Core.Models;

public sealed record AnalysisResult(string Summary, IReadOnlyList<string> Findings);
