using AiAgent.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AiAgent.Infrastructure.Persistence;

public sealed class AgentExecutionDbContext : DbContext
{
    public AgentExecutionDbContext(DbContextOptions<AgentExecutionDbContext> options)
        : base(options)
    {
    }

    public DbSet<AgentExecution> AgentExecutions => Set<AgentExecution>();
}
