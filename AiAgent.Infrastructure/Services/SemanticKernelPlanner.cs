using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelPlanner : IPlanner
{
    private static readonly IReadOnlyList<string> DefaultSteps =
    [
        "Find relevant files",
        "Analyze logic",
        "Identify issue",
        "Generate fix",
        "Generate tests",
        "Validate changes"
    ];

    private readonly IKernelFactory _kernelFactory;
    private readonly ILogger<SemanticKernelPlanner> _logger;

    public SemanticKernelPlanner(IKernelFactory kernelFactory, ILogger<SemanticKernelPlanner> logger)
    {
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<AnalysisResult> CreateAnalysisAsync(
        ProblemRequest request,
        RepositoryAnalysis repository,
        LogSnapshot logs,
        ConfigSnapshot config,
        CancellationToken cancellationToken)
    {
        var kernel = _kernelFactory.CreateKernel();
        var prompt = $"""
            You are a senior AI engineering planner. Create a step-by-step execution plan.
            Task Title: {request.Title}
            Task Description: {request.Description}
            Repository Summary: {repository.Summary}
            Logs Present: {logs.HasContent}
            Config Present: {config.HasContent}

            Return 5-8 short steps in order, one per line.
            """;

        string planText;
        try
        {
            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            planText = result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Planner prompt failed. Using default plan.");
            planText = string.Join(Environment.NewLine, DefaultSteps);
        }

        var steps = planText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(step => step.Trim().TrimStart('-', '*', '•', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ')').Trim())
            .Where(step => !string.IsNullOrWhiteSpace(step))
            .ToList();

        if (steps.Count == 0)
        {
            steps = DefaultSteps.ToList();
        }

        var summary = "Execution plan generated.";
        return new AnalysisResult(summary, steps);
    }
}
