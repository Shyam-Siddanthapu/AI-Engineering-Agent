using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AiAgent.Infrastructure.Services;

public sealed class OllamaClient : ILLMProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options, ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public LlmProvider Provider => LlmProvider.Ollama;

    public async Task<string> GenerateAsync(string prompt, LlmRequestOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return string.Empty;
        }

        var model = string.IsNullOrWhiteSpace(options.Model) ? _options.Model : options.Model;
        var request = new OllamaGenerateRequest(model, prompt, false);
        using var response = await _httpClient.PostAsJsonAsync("api/generate", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Ollama request failed with status {StatusCode}", response.StatusCode);
            return "LLM response unavailable.";
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
        return string.IsNullOrWhiteSpace(result?.Response) ? "LLM response unavailable." : result.Response;
    }

    private sealed record OllamaGenerateRequest(string Model, string Prompt, bool Stream);
    private sealed record OllamaGenerateResponse(string? Response);
}
