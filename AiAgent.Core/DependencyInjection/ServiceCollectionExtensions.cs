using AiAgent.Core.Abstractions;
using AiAgent.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AiAgent.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiAgentCore(this IServiceCollection services)
    {
        services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();
        services.AddSingleton<IPlanner, DefaultPlanner>();
        return services;
    }
}
