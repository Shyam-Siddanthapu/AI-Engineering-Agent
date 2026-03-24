using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelCodeGenerator : ICodeFixer
{
    private const int DefaultMaxContextCharacters = 12000;
    private readonly IKernelFactory _kernelFactory;
    private readonly ICodeContextBuilder _contextBuilder;
    private readonly ILogger<SemanticKernelCodeGenerator> _logger;

    public SemanticKernelCodeGenerator(
        IKernelFactory kernelFactory,
        ICodeContextBuilder contextBuilder,
        ILogger<SemanticKernelCodeGenerator> logger)
    {
        _kernelFactory = kernelFactory;
        _contextBuilder = contextBuilder;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CodeChange>> GenerateFixesAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        CancellationToken cancellationToken)
    {
        var repositoryRequest = new RepositoryRequest
        {
            RepositoryUrl = request.RepositoryUrl ?? string.Empty,
            RepositoryProvider = request.RepositoryProvider,
            Branch = request.Branch
        };

        var context = string.Empty;
        if (!string.IsNullOrWhiteSpace(repositoryRequest.RepositoryUrl))
        {
            context = await _contextBuilder.BuildContextAsync(
                repositoryRequest,
                request.Description,
                DefaultMaxContextCharacters,
                cancellationToken);
        }

        var kernel = _kernelFactory.CreateKernel();
        var prompt = $"""
            You are a senior .NET engineer. Generate code changes for the task.
            Follow clean architecture and .NET best practices. Avoid breaking changes.

            Task Title: {request.Title}
            Task Description: {request.Description}
            Plan Summary: {analysis.Summary}

            Relevant Code Context:
            {context}

            Return updated code in this exact format:
            ### File: relative/path/to/File1.cs
            <complete updated file content>
            ### File: relative/path/to/File2.cs
            <complete updated file content>
            """;

        string resultText;
        try
        {
            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            resultText = result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Code generation failed.");
            return Array.Empty<CodeChange>();
        }

        var changes = ParseChanges(resultText);
        _logger.LogInformation("Generated {ChangeCount} code changes.", changes.Count);
        return changes;
    }

    private static IReadOnlyList<CodeChange> ParseChanges(string content)
    {
        var changes = new List<CodeChange>();
        using var reader = new StringReader(content ?? string.Empty);

        string? line;
        string? currentPath = null;
        var buffer = new StringBuilder();

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.StartsWith("### File:", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("File:", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    changes.Add(new CodeChange
                    {
                        FilePath = currentPath,
                        OriginalCode = string.Empty,
                        ModifiedCode = buffer.ToString().TrimEnd()
                    });
                    buffer.Clear();
                }

                currentPath = line.Split(':', 2, StringSplitOptions.TrimEntries).LastOrDefault();
                continue;
            }

            buffer.AppendLine(line);
        }

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            changes.Add(new CodeChange
            {
                FilePath = currentPath,
                OriginalCode = string.Empty,
                ModifiedCode = buffer.ToString().TrimEnd()
            });
        }

        return changes;
    }
}
