namespace AiAgent.Core.Abstractions;

public interface ILLMClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken);
}
