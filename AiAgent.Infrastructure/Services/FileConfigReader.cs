using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.Services;

public sealed class FileConfigReader : IConfigReader
{
    private readonly ILogger<FileConfigReader> _logger;

    public FileConfigReader(ILogger<FileConfigReader> logger)
    {
        _logger = logger;
    }

    public async Task<ConfigSnapshot> ReadAsync(ProblemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ConfigPath) || !File.Exists(request.ConfigPath))
        {
            _logger.LogInformation("No config file provided or file not found.");
            return new ConfigSnapshot(null);
        }

        var content = await File.ReadAllTextAsync(request.ConfigPath, cancellationToken);
        return new ConfigSnapshot(content);
    }
}
