using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class NoopTestGenerator : ITestGenerator
{
    private readonly ILogger<NoopTestGenerator> _logger;

    public NoopTestGenerator(ILogger<NoopTestGenerator> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<TestArtifact>> GenerateTestsAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        IReadOnlyList<CodeChange> changes,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Test generation not configured.");
        return Task.FromResult<IReadOnlyList<TestArtifact>>(Array.Empty<TestArtifact>());
    }
}
