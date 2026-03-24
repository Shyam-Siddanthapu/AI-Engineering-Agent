using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IChangeApplier
{
    Task<bool> ApplyAsync(IReadOnlyList<CodeChange> changes, CancellationToken cancellationToken);
}
