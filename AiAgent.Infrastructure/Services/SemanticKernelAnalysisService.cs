using AiAgent.Core.Abstractions;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelAnalysisService : IAnalysisService
{
    private readonly IKernelFactory _kernelFactory;
    private readonly ILogger<SemanticKernelAnalysisService> _logger;

    public SemanticKernelAnalysisService(
        IKernelFactory kernelFactory,
        ILogger<SemanticKernelAnalysisService> logger)
    {
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<string> AnalyzeAsync(string context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return "No repository context provided.";
        }

        var kernel = _kernelFactory.CreateKernel();
        var prompt = """
            You are a senior software architect.

            Analyze the following repository context and explain:

            1. What the project does
            2. Key components
            3. Architecture
            4. Technologies used

            Context:
            {{$context}}
            """;

        try
        {
            var result = await kernel.InvokePromptAsync(prompt, new KernelArguments { ["context"] = context }, cancellationToken: cancellationToken);
            return result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analysis prompt failed.");
            return "Analysis could not be completed.";
        }
    }
}
