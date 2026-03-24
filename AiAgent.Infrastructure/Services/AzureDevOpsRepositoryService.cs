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

public sealed class AzureDevOpsRepositoryService : IRepositoryService
{
    private readonly HttpClient _httpClient;
    private readonly AzureDevOpsOptions _options;
    private readonly ILogger<AzureDevOpsRepositoryService> _logger;

    public AzureDevOpsRepositoryService(
        HttpClient httpClient,
        IOptions<AzureDevOpsOptions> options,
        ILogger<AzureDevOpsRepositoryService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetFileListAsync(RepositoryRequest request, CancellationToken cancellationToken)
    {
        var (org, project, repo) = ResolveRepository(request);
        var requestUrl = $"{org}/{project}/_apis/git/repositories/{repo}/items?recursionLevel=Full&api-version=7.0";

        using var response = await SendAsync(request, HttpMethod.Get, requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Azure DevOps file list request failed with status {StatusCode}", response.StatusCode);
            return Array.Empty<string>();
        }

        var payload = await response.Content.ReadFromJsonAsync<AzureDevOpsItemsResponse>(cancellationToken: cancellationToken);
        return payload?.Value
            .Where(item => !item.IsFolder)
            .Select(item => item.Path)
            .ToList() ?? new List<string>();
    }

    public async Task<string?> GetFileContentAsync(RepositoryRequest request, string filePath, CancellationToken cancellationToken)
    {
        var (org, project, repo) = ResolveRepository(request);
        var encodedPath = Uri.EscapeDataString(filePath);
        var requestUrl = $"{org}/{project}/_apis/git/repositories/{repo}/items?path={encodedPath}&includeContent=true&api-version=7.0";

        using var response = await SendAsync(request, HttpMethod.Get, requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Azure DevOps content request failed with status {StatusCode}", response.StatusCode);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<AzureDevOpsItemResponse>(cancellationToken: cancellationToken);
        return payload?.Content;
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

    private (string Organization, string Project, string Repository) ResolveRepository(RepositoryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Organization)
            && !string.IsNullOrWhiteSpace(request.Project)
            && !string.IsNullOrWhiteSpace(request.RepositoryName))
        {
            return (request.Organization, request.Project, request.RepositoryName);
        }

        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            throw new ArgumentException("RepositoryUrl is required.", nameof(request));
        }

        var uri = new Uri(request.RepositoryUrl);
        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3)
        {
            throw new InvalidOperationException("RepositoryUrl is not a valid Azure DevOps repository URL.");
        }

        if (uri.Host.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase))
        {
            var org = segments[0];
            var project = segments.Length > 1 ? segments[1] : request.Project ?? string.Empty;
            var repo = ResolveRepoName(segments, request.RepositoryName);
            return (org, project, repo);
        }

        var hostOrg = uri.Host.Split('.').FirstOrDefault() ?? string.Empty;
        var projectName = segments[0];
        var repository = ResolveRepoName(segments, request.RepositoryName);

        return (hostOrg, projectName, repository);
    }

    private static string ResolveRepoName(string[] segments, string? fallback)
    {
        var repoIndex = Array.IndexOf(segments, "_git");
        if (repoIndex >= 0 && repoIndex + 1 < segments.Length)
        {
            return segments[repoIndex + 1];
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback;
        }

        throw new InvalidOperationException("Repository name could not be resolved from the URL.");
    }

    private Task<HttpResponseMessage> SendAsync(
        RepositoryRequest request,
        HttpMethod method,
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(method, relativeUrl);
        var token = string.IsNullOrWhiteSpace(request.Token) ? _options.Token : request.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
            message.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        }

        return _httpClient.SendAsync(message, cancellationToken);
    }

    private sealed record AzureDevOpsItemsResponse(List<AzureDevOpsItem> Value);
    private sealed record AzureDevOpsItem(string Path, bool IsFolder);
    private sealed record AzureDevOpsItemResponse(string? Content);
}
