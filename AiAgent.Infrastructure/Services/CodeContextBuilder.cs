using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class CodeContextBuilder : ICodeContextBuilder
{
    private static readonly string[] PrioritySegments =
    [
        "service",
        "controller",
        "core",
        "domain",
        "application",
        "handler",
        "processor",
        "workflow"
    ];

    private static readonly string[] SourceExtensions = [".cs", ".csx", ".json", ".yaml", ".yml", ".md"];
    private static readonly string[] PriorityFiles =
    [
        "readme.md",
        "appsettings.json",
        "appsettings.development.json",
        "appsettings.production.json"
    ];
    private readonly IRepositoryService _repositoryService;
    private readonly ILogger<CodeContextBuilder> _logger;

    public CodeContextBuilder(IRepositoryService repositoryService, ILogger<CodeContextBuilder> logger)
    {
        _repositoryService = repositoryService;
        _logger = logger;
    }

    public async Task<string> BuildContextAsync(
        RepositoryRequest repository,
        string query,
        int maxCharacters,
        CancellationToken cancellationToken)
    {
        if (maxCharacters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));
        }

        var limit = Math.Min(maxCharacters, 5000);

        var normalizedQuery = query ?? string.Empty;
        var tokens = normalizedQuery
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var files = await _repositoryService.GetFileListAsync(repository, cancellationToken);
        var candidates = files
            .Where(path => ShouldInclude(path))
            .Select(path => new
            {
                Path = path,
                Score = ScorePath(path, tokens),
                PriorityRank = GetPriorityRank(path)
            })
            .OrderBy(candidate => candidate.PriorityRank)
            .ThenByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidates.Count == 0)
        {
            _logger.LogInformation("No source files found for context.");
            return string.Empty;
        }

        var contextParts = new List<string>();
        var currentSize = 0;

        foreach (var candidate in candidates)
        {
            if (currentSize >= limit)
            {
                break;
            }

            var content = await _repositoryService.GetFileContentAsync(repository, candidate.Path, cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var header = $"\n--- File: {candidate.Path} (score {candidate.Score}) ---\n";
            var remaining = limit - currentSize;
            var snippet = content.Length > remaining - header.Length
                ? content[..Math.Max(0, remaining - header.Length)]
                : content;

            if (snippet.Length == 0)
            {
                break;
            }

            var block = header + snippet;
            contextParts.Add(block);
            currentSize += block.Length;
        }

        _logger.LogInformation("Context builder selected {FileCount} files with {Size} characters.", contextParts.Count, currentSize);
        return string.Join(string.Empty, contextParts);
    }

    private static int ScorePath(string path, string[] tokens)
    {
        var score = 0;
        var normalized = path.ToLowerInvariant();

        if (PriorityFiles.Any(file => normalized.EndsWith(file, StringComparison.OrdinalIgnoreCase)))
        {
            score += 25;
        }

        foreach (var token in tokens)
        {
            if (normalized.Contains(token.ToLowerInvariant()))
            {
                score += 5;
            }
        }

        foreach (var segment in PrioritySegments)
        {
            if (normalized.Contains(segment, StringComparison.OrdinalIgnoreCase))
            {
                score += 3;
            }
        }

        if (normalized.Contains("/controllers/") || normalized.Contains("\\controllers\\"))
        {
            score += 10;
        }

        if (normalized.Contains("/services/") || normalized.Contains("\\services\\"))
        {
            score += 10;
        }

        return score;
    }

    private static int GetPriorityRank(string path)
    {
        var normalized = path.ToLowerInvariant();
        if (normalized.EndsWith("readme.md", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (normalized.Contains("/services/") || normalized.Contains("\\services\\"))
        {
            return 1;
        }

        if (normalized.Contains("/controllers/") || normalized.Contains("\\controllers\\"))
        {
            return 2;
        }

        return 3;
    }

    private static bool ShouldInclude(string path)
    {
        if (PriorityFiles.Any(file => path.EndsWith(file, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return SourceExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
