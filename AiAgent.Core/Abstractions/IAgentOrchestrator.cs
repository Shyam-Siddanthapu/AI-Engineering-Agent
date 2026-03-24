using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IAgentOrchestrator
{
    Task<AgentResponse> ExecuteAsync(ProblemRequest request, CancellationToken cancellationToken);
}
