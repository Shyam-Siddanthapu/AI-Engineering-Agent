using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class LocalRepositoryAnalyzer : IRepositoryAnalyzer
{
    private static readonly string[] IgnoredFolders = [".git", "bin", "obj", ".vs", ".idea"];
    private readonly ILogger<LocalRepositoryAnalyzer> _logger;

    public LocalRepositoryAnalyzer(ILogger<LocalRepositoryAnalyzer> logger)
    {
        _logger = logger;
    }

    public Task<RepositoryAnalysis> AnalyzeAsync(ProblemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            return Task.FromResult(new RepositoryAnalysis("No repository path provided.", Array.Empty<string>()));
        }

        if (!Directory.Exists(request.RepositoryUrl))
        {
            return Task.FromResult(new RepositoryAnalysis("Repository path not found.", Array.Empty<string>()));
        }

        var insights = Directory.EnumerateFiles(request.RepositoryUrl, "*.*", SearchOption.AllDirectories)
            .Where(path => !IgnoredFolders.Any(folder => path.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
            .Take(200)
            .ToList();

        _logger.LogInformation("Repository scan completed with {FileCount} files", insights.Count);

        var summary = $"Scanned {insights.Count} files from {request.RepositoryUrl}.";
        return Task.FromResult(new RepositoryAnalysis(summary, insights));
    }
}
