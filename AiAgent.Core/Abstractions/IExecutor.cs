using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IExecutor
{
    Task<ExecutionResult> ExecuteAsync(
        AgentRequest request,
        int maxContextCharacters,
        CancellationToken cancellationToken);
}
