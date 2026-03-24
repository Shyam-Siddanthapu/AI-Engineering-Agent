using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;

namespace AiAgent.Infrastructure.Services;

public sealed class SemanticKernelTestGenerator : ITestGenerator
{
    private const int DefaultMaxContextCharacters = 10000;
    private readonly IKernelFactory _kernelFactory;
    private readonly ICodeContextBuilder _contextBuilder;
    private readonly ILogger<SemanticKernelTestGenerator> _logger;

    public SemanticKernelTestGenerator(
        IKernelFactory kernelFactory,
        ICodeContextBuilder contextBuilder,
        ILogger<SemanticKernelTestGenerator> logger)
    {
        _kernelFactory = kernelFactory;
        _contextBuilder = contextBuilder;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TestArtifact>> GenerateTestsAsync(
        ProblemRequest request,
        AnalysisResult analysis,
        IReadOnlyList<CodeChange> changes,
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

        var changeList = changes.Count == 0
            ? "No code changes provided."
            : string.Join(Environment.NewLine, changes.Select(change => $"- {change.FilePath}"));

        var kernel = _kernelFactory.CreateKernel();
        var prompt = $"""
            You are a senior .NET test engineer. Generate xUnit tests with Moq.
            Follow clean architecture and .NET best practices. Cover edge cases.

            Task Title: {request.Title}
            Task Description: {request.Description}
            Plan Summary: {analysis.Summary}

            Code Changes:
            {changeList}

            Relevant Code Context:
            {context}

            Return tests in this exact format:
            ### File: relative/path/to/Tests1.cs
            <complete test file content>
            ### File: relative/path/to/Tests2.cs
            <complete test file content>
            """;

        string resultText;
        try
        {
            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            resultText = result.GetValue<string>() ?? result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Test generation failed.");
            return Array.Empty<TestArtifact>();
        }

        var tests = ParseTests(resultText);
        _logger.LogInformation("Generated {TestCount} test artifacts.", tests.Count);
        return tests;
    }

    private static IReadOnlyList<TestArtifact> ParseTests(string content)
    {
        var tests = new List<TestArtifact>();
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
                    tests.Add(new TestArtifact(currentPath, buffer.ToString().TrimEnd()));
                    buffer.Clear();
                }

                currentPath = line.Split(':', 2, StringSplitOptions.TrimEntries).LastOrDefault();
                continue;
            }

            buffer.AppendLine(line);
        }

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            tests.Add(new TestArtifact(currentPath, buffer.ToString().TrimEnd()));
        }

        return tests;
    }
}
