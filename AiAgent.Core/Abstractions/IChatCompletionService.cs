namespace AiAgent.Core.Abstractions;

public interface IChatCompletionService
{
    Task<string> GetCompletionAsync(string prompt, string model, string apiKey, CancellationToken cancellationToken = default);
}
