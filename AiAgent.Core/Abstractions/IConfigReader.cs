using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IConfigReader
{
    Task<ConfigSnapshot> ReadAsync(ProblemRequest request, CancellationToken cancellationToken);
}
