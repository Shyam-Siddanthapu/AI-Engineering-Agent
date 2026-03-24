using AiAgent.Core.Abstractions;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace AiAgent.Infrastructure.Services;

public sealed class OllamaTextGenerationService : ITextGenerationService
{
    private readonly ILLMClient _llmClient;
    private readonly OllamaOptions _options;
    private readonly ILLMRequestContext _context;

    public OllamaTextGenerationService(ILLMClient llmClient, IOptions<OllamaOptions> options, ILLMRequestContext context)
    {
        _llmClient = llmClient;
        _options = options.Value;
        _context = context;
    }

    public string? ServiceId => "ollama";

    public IReadOnlyDictionary<string, object?> Attributes { get; } =
        new Dictionary<string, object?> { ["Model"] = "ollama" };

    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _llmClient.GenerateAsync(prompt, cancellationToken);
        var model = string.IsNullOrWhiteSpace(_context.Current.Model) ? _options.Model : _context.Current.Model;
        var content = new TextContent(response, modelId: model);
        return [content];
    }

    public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Streaming text generation is not configured for Ollama.");
    }
}
