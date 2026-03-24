using AiAgent.Core.Abstractions;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiAgent.Infrastructure.Services;

public sealed class OllamaChatCompletionService : Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService
{
    private readonly ILLMClient _llmClient;
    private readonly OllamaOptions _options;
    private readonly ILLMRequestContext _context;

    public OllamaChatCompletionService(ILLMClient llmClient, IOptions<OllamaOptions> options, ILLMRequestContext context)
    {
        _llmClient = llmClient;
        _options = options.Value;
        _context = context;
    }

    public string? ServiceId => "ollama";

    public IReadOnlyDictionary<string, object?> Attributes { get; } =
        new Dictionary<string, object?> { ["Model"] = "ollama" };

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = string.Join(Environment.NewLine, chatHistory.Select(message =>
            $"{message.Role}: {message.Content}"));

        var response = await _llmClient.GenerateAsync(prompt, cancellationToken);
        var model = string.IsNullOrWhiteSpace(_context.Current.Model) ? _options.Model : _context.Current.Model;
        var content = new ChatMessageContent(AuthorRole.Assistant, response, modelId: model);
        return [content];
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Streaming chat is not configured for Ollama.");
    }
}
