using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class NoopCodeFixer : ICodeFixer
{
    private readonly ILogger<NoopCodeFixer> _logger;

    public NoopCodeFixer(ILogger<NoopCodeFixer> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<CodeChange>> GenerateFixesAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Code fix generation not configured.");
        return Task.FromResult<IReadOnlyList<CodeChange>>(Array.Empty<CodeChange>());
    }
}
