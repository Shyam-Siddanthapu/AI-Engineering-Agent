using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IRepositoryService
{
    Task<IReadOnlyList<string>> GetFileListAsync(RepositoryRequest request, CancellationToken cancellationToken);
    Task<string?> GetFileContentAsync(RepositoryRequest request, string filePath, CancellationToken cancellationToken);
    Task<string> CloneAsync(RepositoryRequest request, string targetDirectory, CancellationToken cancellationToken);
}
