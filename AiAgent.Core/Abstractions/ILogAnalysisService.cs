using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface ILogAnalysisService
{
    Task<LogAnalysisResult> AnalyzeAsync(string logContent, CancellationToken cancellationToken);
}
