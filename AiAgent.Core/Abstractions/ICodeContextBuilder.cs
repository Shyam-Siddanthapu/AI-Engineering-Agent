using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ICodeContextBuilder
{
    Task<string> BuildContextAsync(
        RepositoryRequest repository,
        string query,
        int maxCharacters,
        CancellationToken cancellationToken);
}
