using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IPlanner
{
    Task<AnalysisResult> CreateAnalysisAsync(
        ProblemRequest request,
        RepositoryAnalysis repository,
        LogSnapshot logs,
        ConfigSnapshot config,
        CancellationToken cancellationToken);
}
