using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace AiAgent.Infrastructure.Services;

public sealed class GitHubRepositoryService : IRepositoryService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;
    private readonly ILogger<GitHubRepositoryService> _logger;

    public GitHubRepositoryService(HttpClient httpClient, IOptions<GitHubOptions> options, ILogger<GitHubRepositoryService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AiAgent", "1.0"));
    }

    public async Task<IReadOnlyList<string>> GetFileListAsync(RepositoryRequest request, CancellationToken cancellationToken)
    {
        var (owner, repo) = ParseRepository(request);
        var branch = string.IsNullOrWhiteSpace(request.Branch) ? "main" : request.Branch;

        using var response = await SendAsync(request, HttpMethod.Get, $"repos/{owner}/{repo}/git/trees/{branch}?recursive=1", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub file list request failed with status {StatusCode}", response.StatusCode);
            return Array.Empty<string>();
        }

        var payload = await response.Content.ReadFromJsonAsync<GitHubTreeResponse>(cancellationToken: cancellationToken);
        return payload?.Tree
            .Where(node => node.Type == "blob" && !string.IsNullOrWhiteSpace(node.Path))
            .Select(node => node.Path!)
            .ToList() ?? new List<string>();
    }

    public async Task<string?> GetFileContentAsync(RepositoryRequest request, string filePath, CancellationToken cancellationToken)
    {
        var (owner, repo) = ParseRepository(request);
        var branch = string.IsNullOrWhiteSpace(request.Branch) ? "main" : request.Branch;

        using var response = await SendAsync(request, HttpMethod.Get, $"repos/{owner}/{repo}/contents/{filePath}?ref={branch}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub content request failed with status {StatusCode}", response.StatusCode);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<GitHubContentResponse>(cancellationToken: cancellationToken);
        if (payload?.Content is null)
        {
            return null;
        }

        var normalized = payload.Content.Replace("\n", string.Empty);
        var bytes = Convert.FromBase64String(normalized);
        return Encoding.UTF8.GetString(bytes);
    }

    public async Task<string> CloneAsync(RepositoryRequest request, string targetDirectory, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            throw new ArgumentException("RepositoryUrl is required to clone.", nameof(request));
        }

        Directory.CreateDirectory(targetDirectory);
        var cloneUrl = request.RepositoryUrl;

        var processStart = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"clone {cloneUrl} \"{targetDirectory}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(processStart);
        if (process is null)
        {
            throw new InvalidOperationException("Unable to start git process.");
        }

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"Git clone failed: {error}");
        }

        return targetDirectory;
    }

    private (string Owner, string Repo) ParseRepository(RepositoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            throw new ArgumentException("RepositoryUrl is required.", nameof(request));
        }

        var uri = new Uri(request.RepositoryUrl);
        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            throw new InvalidOperationException("RepositoryUrl is not a valid GitHub repository URL.");
        }

        return (segments[0], segments[1].Replace(".git", string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private Task<HttpResponseMessage> SendAsync(
        RepositoryRequest request,
        HttpMethod method,
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(method, relativeUrl);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var token = string.IsNullOrWhiteSpace(request.Token) ? _options.Token : request.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return _httpClient.SendAsync(message, cancellationToken);
    }

    private sealed record GitHubTreeResponse(List<GitHubTreeNode> Tree);
    private sealed record GitHubTreeNode(string? Path, string Type);
    private sealed record GitHubContentResponse(string? Content, string Encoding);
}
