using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class RepositoryService : IRepositoryService
{
    private readonly GitHubRepositoryService _gitHubRepositoryService;
    private readonly AzureDevOpsRepositoryService _azureDevOpsRepositoryService;

    public RepositoryService(
        GitHubRepositoryService gitHubRepositoryService,
        AzureDevOpsRepositoryService azureDevOpsRepositoryService)
    {
        _gitHubRepositoryService = gitHubRepositoryService;
        _azureDevOpsRepositoryService = azureDevOpsRepositoryService;
    }

    public Task<IReadOnlyList<string>> GetFileListAsync(RepositoryRequest request, CancellationToken cancellationToken)
        => ResolveProvider(request).GetFileListAsync(request, cancellationToken);

    public Task<string?> GetFileContentAsync(RepositoryRequest request, string filePath, CancellationToken cancellationToken)
        => ResolveProvider(request).GetFileContentAsync(request, filePath, cancellationToken);

    public Task<string> CloneAsync(RepositoryRequest request, string targetDirectory, CancellationToken cancellationToken)
        => ResolveProvider(request).CloneAsync(request, targetDirectory, cancellationToken);

    private IRepositoryService ResolveProvider(RepositoryRequest request)
    {
        var provider = request.RepositoryProvider?.Trim() ?? string.Empty;

        return provider.Equals("azuredevops", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("azdo", StringComparison.OrdinalIgnoreCase)
            || request.RepositoryUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
            || request.RepositoryUrl.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase)
            ? _azureDevOpsRepositoryService
            : _gitHubRepositoryService;
    }
}
