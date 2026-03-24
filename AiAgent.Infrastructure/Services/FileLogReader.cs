using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class FileLogReader : ILogReader
{
    private readonly ILogger<FileLogReader> _logger;

    public FileLogReader(ILogger<FileLogReader> logger)
    {
        _logger = logger;
    }

    public async Task<LogSnapshot> ReadAsync(ProblemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LogsPath) || !File.Exists(request.LogsPath))
        {
            _logger.LogInformation("No log file provided or file not found.");
            return new LogSnapshot(null);
        }

        var content = await File.ReadAllTextAsync(request.LogsPath, cancellationToken);
        return new LogSnapshot(content);
    }
}
