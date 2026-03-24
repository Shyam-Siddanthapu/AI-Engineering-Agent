using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class LlmClient : ILLMClient
{
    private readonly IEnumerable<ILLMProviderClient> _providers;
    private readonly ILLMRequestContext _context;
    private readonly ILogger<LlmClient> _logger;

    public LlmClient(
        IEnumerable<ILLMProviderClient> providers,
        ILLMRequestContext context,
        ILogger<LlmClient> logger)
    {
        _providers = providers;
        _context = context;
        _logger = logger;
    }

    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var options = _context.Current;
        var provider = _providers.FirstOrDefault(p => p.Provider == options.Provider);

        if (provider is null)
        {
            _logger.LogWarning("No LLM provider registered for {Provider}", options.Provider);
            return Task.FromResult("LLM provider unavailable.");
        }

        return provider.GenerateAsync(prompt, options, cancellationToken);
    }
}
