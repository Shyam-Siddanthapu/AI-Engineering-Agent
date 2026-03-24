using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IAgentExecutionRepository
{
    Task AddAsync(AgentExecution execution, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgentExecution>> GetAllAsync(CancellationToken cancellationToken);
    Task<AgentExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
