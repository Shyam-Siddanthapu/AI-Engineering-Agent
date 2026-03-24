using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Core.Services;

public sealed class DefaultPlanner : IPlanner
{
    private readonly ILLMClient _llmClient;
    private readonly ILogger<DefaultPlanner> _logger;

    public DefaultPlanner(ILLMClient llmClient, ILogger<DefaultPlanner> logger)
    {
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task<AnalysisResult> CreateAnalysisAsync(
        ProblemRequest request,
        RepositoryAnalysis repository,
        LogSnapshot logs,
        ConfigSnapshot config,
        CancellationToken cancellationToken)
    {
        var prompt = $"Analyze the request '{request.Title}'. Description: {request.Description}. " +
                     $"Repository summary: {repository.Summary}. " +
                     $"Logs present: {logs.HasContent}. Config present: {config.HasContent}.";

        string summary;
        try
        {
            summary = await _llmClient.GenerateAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM analysis failed, falling back to default summary.");
            summary = "Analysis completed with fallback planner.";
        }

        var findings = new List<string>
        {
            repository.Summary,
            logs.HasContent ? "Logs loaded for analysis." : "No logs provided.",
            config.HasContent ? "Configuration loaded for analysis." : "No configuration provided."
        };

        return new AnalysisResult(summary, findings);
    }
}
