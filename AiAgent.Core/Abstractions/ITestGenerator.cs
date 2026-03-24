using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ITestGenerator
{
    Task<IReadOnlyList<TestArtifact>> GenerateTestsAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        IReadOnlyList<CodeChange> changes,
        CancellationToken cancellationToken);
}
