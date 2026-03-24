using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AiAgent.Infrastructure.Services;

public sealed class RepoWorkspaceManager
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private readonly string _baseFolder;

    public RepoWorkspaceManager()
    {
        _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "workspaces");
    }

    public async Task<string> GetOrCreateWorkspace(string repoUrl)
    {
        if (string.IsNullOrWhiteSpace(repoUrl))
        {
            throw new ArgumentException("Repository URL is required.", nameof(repoUrl));
        }

        Directory.CreateDirectory(_baseFolder);

        var folderName = BuildWorkspaceFolder(repoUrl);
        var targetPath = Path.Combine(_baseFolder, folderName);
        var gate = Locks.GetOrAdd(targetPath, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync();
        try
        {
            if (!Directory.Exists(targetPath) || !Directory.EnumerateFileSystemEntries(targetPath).Any())
            {
                await RunGitAsync($"clone {repoUrl} \"{targetPath}\"", targetPath);
                return targetPath;
            }

            await RunGitAsync($"-C \"{targetPath}\" pull", targetPath);
            return targetPath;
        }
        finally
        {
            gate.Release();
        }
    }

    private static string BuildWorkspaceFolder(string repoUrl)
    {
        var normalized = repoUrl.Trim().TrimEnd('/');
        var repoName = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "repo";
        repoName = repoName.Replace(".git", string.Empty, StringComparison.OrdinalIgnoreCase);

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        var suffix = Convert.ToHexString(hash)[..8].ToLowerInvariant();

        return $"{Sanitize(repoName)}-{suffix}";
    }

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        return builder.Length == 0 ? "repo" : builder.ToString();
    }

    private static async Task RunGitAsync(string arguments, string workingDirectory)
    {
        var processStart = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        using var process = Process.Start(processStart);
        if (process is null)
        {
            throw new InvalidOperationException("Unable to start git process.");
        }

        var stdOut = await process.StandardOutput.ReadToEndAsync();
        var stdErr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed: {stdErr}{Environment.NewLine}{stdOut}");
        }
    }
}
