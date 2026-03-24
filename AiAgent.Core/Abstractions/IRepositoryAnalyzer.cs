using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IRepositoryAnalyzer
{
    Task<RepositoryAnalysis> AnalyzeAsync(ProblemRequest request, CancellationToken cancellationToken);
}
