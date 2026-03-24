using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class NoopChangeApplier : IChangeApplier
{
    private readonly ILogger<NoopChangeApplier> _logger;

    public NoopChangeApplier(ILogger<NoopChangeApplier> logger)
    {
        _logger = logger;
    }

    public Task<bool> ApplyAsync(IReadOnlyList<CodeChange> changes, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Change application is not configured.");
        return Task.FromResult(false);
    }
}
