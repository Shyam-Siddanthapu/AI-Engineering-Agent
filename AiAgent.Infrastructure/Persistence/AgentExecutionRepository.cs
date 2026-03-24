using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AiAgent.Infrastructure.Persistence;

public sealed class AgentExecutionRepository : IAgentExecutionRepository
{
    private readonly AgentExecutionDbContext _dbContext;

    public AgentExecutionRepository(AgentExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AgentExecution execution, CancellationToken cancellationToken)
    {
        _dbContext.AgentExecutions.Add(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgentExecution>> GetAllAsync(CancellationToken cancellationToken)
    {
        var results = await _dbContext.AgentExecutions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return results
            .OrderByDescending(execution => execution.CreatedAt)
            .ToList();
    }

    public async Task<AgentExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.AgentExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(execution => execution.Id == id, cancellationToken);
    }
}
