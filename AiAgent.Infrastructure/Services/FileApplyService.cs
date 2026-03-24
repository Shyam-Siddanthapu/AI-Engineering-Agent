using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AiAgent.Infrastructure.Services;

public sealed class FileApplyService : IChangeApplier
{
    private readonly FileApplyOptions _options;
    private readonly ILogger<FileApplyService> _logger;
    private readonly MockFileService _mockFileService;

    public FileApplyService(
        IOptions<FileApplyOptions> options,
        MockFileService mockFileService,
        ILogger<FileApplyService> logger)
    {
        _options = options.Value;
        _mockFileService = mockFileService;
        _logger = logger;
    }

    public Task<bool> ApplyAsync(IReadOnlyList<CodeChange> changes, CancellationToken cancellationToken)
        => ApplyChanges(Directory.GetCurrentDirectory(), changes, ExecutionMode.Apply, "Ollama", cancellationToken);

    public async Task<bool> ApplyChanges(
        string repoPath,
        IReadOnlyList<CodeChange> changes,
        ExecutionMode mode,
        string provider,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            throw new ArgumentException("Repository path is required.", nameof(repoPath));
        }

        if (changes.Count == 0)
        {
            _logger.LogInformation("No changes to apply.");
            return true;
        }

        if (mode == ExecutionMode.Preview || _options.PreviewOnly)
        {
            _logger.LogInformation("Preview mode enabled. Skipping file writes.");
            return true;
        }

        if (string.Equals(provider, "Mock", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var change in changes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _mockFileService.UpdateFile(change.FilePath, change.ModifiedCode);
            }

            _logger.LogInformation("Applied {Count} mock changes.", changes.Count);
            return true;
        }

        if (mode == ExecutionMode.Git)
        {
            var branchName = $"ai-agent/{DateTime.UtcNow:yyyyMMddHHmmss}";
            await RunGitAsync($"-C \"{repoPath}\" checkout -b {branchName}", cancellationToken);
        }

        foreach (var change in changes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(change.FilePath))
            {
                continue;
            }

            var targetPath = Path.GetFullPath(Path.Combine(repoPath, change.FilePath));
            var targetDirectory = Path.GetDirectoryName(targetPath);

            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                _logger.LogWarning("Invalid target directory for {FilePath}", change.FilePath);
                continue;
            }

            Directory.CreateDirectory(targetDirectory);

            if (File.Exists(targetPath))
            {
                var backupPath = targetPath + ".bak";
                File.Copy(targetPath, backupPath, overwrite: true);
            }

            await File.WriteAllTextAsync(targetPath, change.ModifiedCode, cancellationToken);
            _logger.LogInformation("Applied change to {FilePath}", change.FilePath);
        }

        if (mode == ExecutionMode.Git)
        {
            await RunGitAsync($"-C \"{repoPath}\" add .", cancellationToken);
            await RunGitAsync($"-C \"{repoPath}\" commit -m \"{_options.CommitMessage}\"", cancellationToken);
        }

        return true;
    }

    private async Task RunGitAsync(string arguments, CancellationToken cancellationToken)
    {
        var processStart = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(processStart);
        if (process is null)
        {
            _logger.LogWarning("Unable to start git process for {Arguments}", arguments);
            return;
        }

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            _logger.LogWarning("Git command failed: {Error}", error);
        }
    }
}
