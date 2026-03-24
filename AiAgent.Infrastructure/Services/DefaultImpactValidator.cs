using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class DefaultImpactValidator : IImpactValidator
{
    private readonly ILogger<DefaultImpactValidator> _logger;

    public DefaultImpactValidator(ILogger<DefaultImpactValidator> logger)
    {
        _logger = logger;
    }

    public Task<ValidationResult> ValidateAsync(
        ProblemRequest request,
        IReadOnlyList<CodeChange> changes,
        IReadOnlyList<TestArtifact> tests,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Impact validation completed with no automated checks.");
        var notes = new List<string> { "No automated impact validation configured." };
        return Task.FromResult(new ValidationResult(true, notes));
    }
}
