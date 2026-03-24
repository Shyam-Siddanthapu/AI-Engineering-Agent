using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelLogAnalysisService : ILogAnalysisService
{
    private readonly IKernelFactory _kernelFactory;
    private readonly ILogger<SemanticKernelLogAnalysisService> _logger;

    public SemanticKernelLogAnalysisService(
        IKernelFactory kernelFactory,
        ILogger<SemanticKernelLogAnalysisService> logger)
    {
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<LogAnalysisResult> AnalyzeAsync(string logContent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(logContent))
        {
            return new LogAnalysisResult("No logs provided.", Array.Empty<string>(), "Provide log content for analysis.");
        }

        var kernel = _kernelFactory.CreateKernel();
        var prompt = $"""
            You are a senior .NET incident responder. Analyze the logs and identify root cause.
            Provide affected components and a suggested fix.

            Logs:
            {logContent}

            Respond with:
            ROOT_CAUSE: <root cause>
            COMPONENTS: <comma-separated components>
            SUGGESTED_FIX: <suggested fix>
            """;

        string responseText;
        try
        {
            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            responseText = result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Log analysis prompt failed.");
            return new LogAnalysisResult("Unable to analyze logs.", Array.Empty<string>(), "Retry with additional context.");
        }

        var rootCause = ExtractValue(responseText, "ROOT_CAUSE") ?? "Root cause not identified.";
        var componentsText = ExtractValue(responseText, "COMPONENTS") ?? string.Empty;
        var suggestedFix = ExtractValue(responseText, "SUGGESTED_FIX") ?? "No suggestion provided.";

        var components = componentsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new LogAnalysisResult(rootCause, components, suggestedFix);
    }

    private static string? ExtractValue(string content, string key)
    {
        var lines = content
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase))
            {
                return line[(key.Length + 1)..].Trim();
            }
        }

        return null;
    }
}
