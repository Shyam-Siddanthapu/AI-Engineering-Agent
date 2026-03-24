using AiAgent.Core.Abstractions;
using AiAgent.Infrastructure.Abstractions;
using AiAgent.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;

namespace AiAgent.Infrastructure.Services;

public sealed class KernelFactory : IKernelFactory
{
    private readonly IOptions<OllamaOptions> _optionsAccessor;
    private readonly ILLMClient _llmClient;
    private readonly ILLMRequestContext _context;
    private readonly ILogger<KernelFactory> _logger;

    public KernelFactory(
        IOptions<OllamaOptions> options,
        ILLMClient llmClient,
        ILLMRequestContext context,
        ILogger<KernelFactory> logger)
    {
        _optionsAccessor = options;
        _llmClient = llmClient;
        _context = context;
        _logger = logger;
    }

    public Kernel CreateKernel(IEnumerable<KernelPlugin>? plugins = null)
    {
        var options = _optionsAccessor.Value;
        var model = string.IsNullOrWhiteSpace(options.Model) ? "llama3" : options.Model;
        var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl) ? "http://localhost:11434" : options.BaseUrl;

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException($"Invalid Ollama base URL: {baseUrl}");
        }

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_llmClient);
        builder.Services.AddSingleton(_optionsAccessor);
        builder.Services.AddSingleton(_context);
        builder.Services.AddSingleton<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService, OllamaChatCompletionService>();
        builder.Services.AddSingleton<Microsoft.SemanticKernel.TextGeneration.ITextGenerationService, OllamaTextGenerationService>();

        var kernel = builder.Build();

        if (plugins is not null)
        {
            foreach (var plugin in plugins)
            {
                kernel.Plugins.Add(plugin);
            }
        }

        _logger.LogInformation("Semantic Kernel built with model {Model} at {BaseUrl}", model, baseUri);
        return kernel;
    }
}
