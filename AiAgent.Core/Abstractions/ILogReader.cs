using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ILogReader
{
    Task<LogSnapshot> ReadAsync(ProblemRequest request, CancellationToken cancellationToken);
}
