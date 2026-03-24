namespace AiAgent.Core.Abstractions;

public interface IAnalysisService
{
    Task<string> AnalyzeAsync(string context, CancellationToken cancellationToken);
}
