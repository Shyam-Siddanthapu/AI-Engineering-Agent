using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ICodeFixer
{
    Task<IReadOnlyList<CodeChange>> GenerateFixesAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        CancellationToken cancellationToken);
}
