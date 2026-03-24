using AiAgent.Core.Abstractions;
using AiAgent.Infrastructure.Abstractions;
using AiAgent.Infrastructure.Llm;
using AiAgent.Infrastructure.Options;
using AiAgent.Infrastructure.Persistence;
using AiAgent.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace AiAgent.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));
        services.Configure<GroqOptions>(configuration.GetSection("Groq"));
        services.Configure<AzureOpenAiOptions>(configuration.GetSection("AzureOpenAI"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));
        services.Configure<AzureDevOpsOptions>(configuration.GetSection("AzureDevOps"));
        services.Configure<FileApplyOptions>(configuration.GetSection("FileApply"));

        services.AddHttpClient<OllamaClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });

        services.AddHttpClient<GroqClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GroqOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });

        services.AddHttpClient<AzureOpenAiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AzureOpenAiOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });

        services.AddHttpClient<GitHubRepositoryService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GitHubOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }

            if (!string.IsNullOrWhiteSpace(options.Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Token);
            }
        });

        services.AddHttpClient<AzureDevOpsRepositoryService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AzureDevOpsOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }

            if (!string.IsNullOrWhiteSpace(options.Token))
            {
                var token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{options.Token}"));
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);
            }
        });

        services.AddSingleton<IRepositoryAnalyzer, LocalRepositoryAnalyzer>();
        services.AddSingleton<ILogReader, FileLogReader>();
        services.AddSingleton<IConfigReader, FileConfigReader>();
        services.AddSingleton<ICodeFixer, SemanticKernelCodeGenerator>();
        services.AddSingleton<ITestGenerator, SemanticKernelTestGenerator>();
        services.AddSingleton<IImpactValidator, SemanticKernelValidationService>();
        services.AddSingleton<IChangeApplier, FileApplyService>();
        services.AddSingleton<FileApplyService>();
        services.AddSingleton<ILLMRequestContext, LlmRequestContext>();
        services.AddSingleton<ILLMClient, LlmClient>();
        services.AddSingleton<ILLMProviderClient, OllamaClient>();
        services.AddSingleton<ILLMProviderClient, GroqClient>();
        services.AddSingleton<ILLMProviderClient, AzureOpenAiClient>();
        services.AddSingleton<ILLMProviderClient, MockLlmClient>();
        services.AddSingleton<IKernelFactory, KernelFactory>();
        services.AddSingleton<IPlanner, SemanticKernelPlanner>();
        services.AddSingleton<ILogAnalysisService, SemanticKernelLogAnalysisService>();
        services.AddSingleton<IAnalysisService, SemanticKernelAnalysisService>();
        services.AddSingleton<IIntentClassifier, IntentClassifier>();
        services.AddSingleton<IPromptBuilder, SuperPromptBuilder>();
        services.AddTransient<IRepositoryService, RepositoryService>();
        services.AddTransient<ICodeContextBuilder, CodeContextBuilder>();
        services.AddTransient<IExecutor, Executor>();
        services.AddSingleton<global::AiAgent.Core.Abstractions.IChatCompletionService, global::AiAgent.Infrastructure.Llm.OllamaChatCompletionService>();
        services.AddSingleton<global::AiAgent.Core.Abstractions.IChatCompletionService, global::AiAgent.Infrastructure.Llm.AzureOpenAIChatCompletionService>();
        services.AddSingleton<global::AiAgent.Core.Abstractions.IChatCompletionService, global::AiAgent.Infrastructure.Llm.GroqChatCompletionService>();
        services.AddSingleton<global::AiAgent.Core.Abstractions.IChatCompletionService, global::AiAgent.Infrastructure.Llm.MockChatCompletionService>();
        services.AddSingleton<global::AiAgent.Infrastructure.Llm.OllamaChatCompletionService>();
        services.AddSingleton<global::AiAgent.Infrastructure.Llm.AzureOpenAIChatCompletionService>();
        services.AddSingleton<global::AiAgent.Infrastructure.Llm.GroqChatCompletionService>();
        services.AddSingleton<global::AiAgent.Infrastructure.Llm.MockChatCompletionService>();
        services.AddSingleton<RepoWorkspaceManager>();
        services.AddSingleton<DiffService>();
        services.AddSingleton<MockFileService>();
        services.AddDbContext<AgentExecutionDbContext>(options =>
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "agent-executions.db");
            options.UseSqlite($"Data Source={dbPath}");
        });
        services.AddScoped<IAgentExecutionRepository, AgentExecutionRepository>();
        services.AddSingleton<global::AiAgent.Infrastructure.Llm.ChatCompletionFactory>();

        return services;
    }
}
