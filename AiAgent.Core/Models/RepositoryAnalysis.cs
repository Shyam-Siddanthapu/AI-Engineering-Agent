namespace AiAgent.Core.Models;

public sealed record RepositoryAnalysis(string Summary, IReadOnlyList<string> Insights);
