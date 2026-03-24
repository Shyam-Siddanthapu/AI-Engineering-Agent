using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IImpactValidator
{
    Task<ValidationResult> ValidateAsync(
        ProblemRequest request,
        IReadOnlyList<CodeChange> changes,
        IReadOnlyList<TestArtifact> tests,
        CancellationToken cancellationToken);
}
