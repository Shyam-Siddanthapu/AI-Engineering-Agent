using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Core.Utilities;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiAgent.Infrastructure.Services;

public sealed class GroqClient : ILLMProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly GroqOptions _options;
    private readonly ILogger<GroqClient> _logger;

    public GroqClient(HttpClient httpClient, IOptions<GroqOptions> options, ILogger<GroqClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public LlmProvider Provider => LlmProvider.Groq;

    public async Task<string> GenerateAsync(string prompt, LlmRequestOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            _logger.LogWarning("Groq API key missing. Provided: {ApiKey}", ApiKeyMasker.Mask(options.ApiKey));
            return "Groq API key missing.";
        }

        var model = string.IsNullOrWhiteSpace(options.Model) ? "llama3-8b-8192" : options.Model;
        var request = new GroqChatRequest(model, [new GroqMessage("user", prompt)]);

        using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Groq request failed with status {StatusCode}", response.StatusCode);
            return "LLM response unavailable.";
        }

        var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(cancellationToken: cancellationToken);
        var content = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        return string.IsNullOrWhiteSpace(content) ? "LLM response unavailable." : content;
    }

    private sealed record GroqChatRequest(string Model, List<GroqMessage> Messages);
    private sealed record GroqMessage(string Role, string Content);

    private sealed record GroqChatResponse(List<GroqChoice>? Choices);
    private sealed record GroqChoice(GroqMessage? Message);
}
