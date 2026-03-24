using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelValidationService : IImpactValidator
{
    private readonly IKernelFactory _kernelFactory;
    private readonly ILogger<SemanticKernelValidationService> _logger;

    public SemanticKernelValidationService(
        IKernelFactory kernelFactory,
        ILogger<SemanticKernelValidationService> logger)
    {
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAsync(
        ProblemRequest request,
        IReadOnlyList<CodeChange> changes,
        IReadOnlyList<TestArtifact> tests,
        CancellationToken cancellationToken)
    {
        if (changes.Count == 0)
        {
            return new ValidationResult(true, ["No code changes to validate."]);
        }

        var changeSummary = string.Join(Environment.NewLine, changes.Select(change => $"- {change.FilePath}"));
        var testSummary = tests.Count == 0
            ? "No tests generated."
            : string.Join(Environment.NewLine, tests.Select(test => $"- {test.FilePath}"));

        var kernel = _kernelFactory.CreateKernel();
        var prompt = $"""
            You are a senior .NET reviewer. Review the proposed changes for breaking changes, missing dependencies,
            and improvements. Be concise and precise.

            Task Title: {request.Title}
            Task Description: {request.Description}

            Code Changes:
            {changeSummary}

            Tests:
            {testSummary}

            Respond with one item per line using these prefixes:
            BREAKING: <describe breaking change>
            MISSING: <describe missing dependency>
            IMPROVEMENT: <suggest improvement>
            OK: <confirmation when no issues>
            """;

        string responseText;
        try
        {
            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            responseText = result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Validation prompt failed. Assuming pending validation.");
            return new ValidationResult(false, ["Validation could not be completed."]);
        }

        var notes = responseText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (notes.Count == 0)
        {
            notes.Add("No validation findings returned.");
        }

        var hasBreaking = notes.Any(note => note.StartsWith("BREAKING:", StringComparison.OrdinalIgnoreCase));
        var hasMissing = notes.Any(note => note.StartsWith("MISSING:", StringComparison.OrdinalIgnoreCase));

        if (!hasBreaking && !hasMissing && !notes.Any(note => note.StartsWith("OK:", StringComparison.OrdinalIgnoreCase)))
        {
            notes.Insert(0, "OK: No breaking changes or missing dependencies detected.");
        }

        return new ValidationResult(!hasBreaking && !hasMissing, notes);
    }
}
