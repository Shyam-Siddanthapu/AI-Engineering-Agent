using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Core.Utilities;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiAgent.Infrastructure.Services;

public sealed class AzureOpenAiClient : ILLMProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAiOptions _options;
    private readonly ILogger<AzureOpenAiClient> _logger;

    public AzureOpenAiClient(HttpClient httpClient, IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public LlmProvider Provider => LlmProvider.AzureOpenAI;

    public async Task<string> GenerateAsync(string prompt, LlmRequestOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            _logger.LogWarning("Azure OpenAI API key missing. Provided: {ApiKey}", ApiKeyMasker.Mask(options.ApiKey));
            return "Azure OpenAI API key missing.";
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return "Azure OpenAI base URL not configured.";
        }

        var deployment = string.IsNullOrWhiteSpace(options.Model) ? "gpt-4o-mini" : options.Model;
        var request = new AzureChatRequest([new AzureMessage("user", prompt)]);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"openai/deployments/{deployment}/chat/completions?api-version={_options.ApiVersion}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("api-key", options.ApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Azure OpenAI request failed with status {StatusCode}", response.StatusCode);
            return "LLM response unavailable.";
        }

        var payload = await response.Content.ReadFromJsonAsync<AzureChatResponse>(cancellationToken: cancellationToken);
        var content = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        return string.IsNullOrWhiteSpace(content) ? "LLM response unavailable." : content;
    }

    private sealed record AzureChatRequest(List<AzureMessage> Messages);
    private sealed record AzureMessage(string Role, string Content);

    private sealed record AzureChatResponse(List<AzureChoice>? Choices);
    private sealed record AzureChoice(AzureMessage? Message);
}
