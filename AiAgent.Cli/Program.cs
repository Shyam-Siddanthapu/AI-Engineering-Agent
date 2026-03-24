using AiAgent.Core.Abstractions;
using AiAgent.Core.DependencyInjection;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.CommandLine;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddAiAgentCore();
builder.Services.AddAiAgentInfrastructure(builder.Configuration);
builder.Logging.ClearProviders();
builder.Services.AddSerilog((serviceProvider, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AiAgent.Cli");

var repoOption = new Option<string>("--repo", "Repository URL") { IsRequired = true };
var taskOption = new Option<string>("--task", "Task description") { IsRequired = true };
var providerOption = new Option<string?>("--provider", () => null, "Repository provider (github/azuredevops)");
var llmProviderOption = new Option<string>("--llm-provider", () => "Ollama", "LLM provider (Ollama/AzureOpenAI/Groq/Mock)");
var llmModelOption = new Option<string?>("--llm-model", () => null, "LLM model or deployment name");
var llmApiKeyOption = new Option<string?>("--llm-api-key", () => null, "LLM API key (not stored)");
var branchOption = new Option<string?>("--branch", () => null, "Repository branch");
var logsOption = new Option<string?>("--logs", () => null, "Log content or path to a log file");
var maxContextOption = new Option<int>("--max-context", () => 12000, "Max context characters");

var analyzeCommand = new Command("analyze", "Generate a step-by-step plan")
{
    repoOption,
    taskOption,
    providerOption,
    branchOption,
    logsOption,
    llmProviderOption,
    llmModelOption,
    llmApiKeyOption
};

analyzeCommand.SetHandler(async context =>
{
    var repo = context.ParseResult.GetValueForOption(repoOption);
    var task = context.ParseResult.GetValueForOption(taskOption);
    var provider = context.ParseResult.GetValueForOption(providerOption);
    var branch = context.ParseResult.GetValueForOption(branchOption);
    var logs = context.ParseResult.GetValueForOption(logsOption);
    var llmProvider = context.ParseResult.GetValueForOption(llmProviderOption);
    var llmModel = context.ParseResult.GetValueForOption(llmModelOption);
    var llmApiKey = context.ParseResult.GetValueForOption(llmApiKeyOption);

    var planner = host.Services.GetRequiredService<IPlanner>();
    var repositoryService = host.Services.GetRequiredService<IRepositoryService>();
    var logAnalyzer = host.Services.GetRequiredService<ILogAnalysisService>();
    var llmContext = host.Services.GetRequiredService<ILLMRequestContext>();

    llmContext.Set(new LlmRequestOptions
    {
        Provider = Enum.TryParse<LlmProvider>(llmProvider, true, out var parsedProvider) ? parsedProvider : LlmProvider.Ollama,
        Model = llmModel,
        ApiKey = llmApiKey
    });

    var request = new ProblemRequest
    {
        Title = task,
        Description = task,
        Type = ProblemType.Incident,
        RepositoryUrl = repo,
        RepositoryProvider = provider,
        Branch = branch,
        ApplyChanges = false
    };

    var repositoryRequest = new RepositoryRequest
    {
        RepositoryUrl = repo,
        RepositoryProvider = provider,
        Branch = branch
    };

    var files = await repositoryService.GetFileListAsync(repositoryRequest, CancellationToken.None);
    var repositoryAnalysis = new RepositoryAnalysis($"Repository returned {files.Count} files.", files);
    var plan = await planner.CreateAnalysisAsync(
        request,
        repositoryAnalysis,
        new LogSnapshot(logs),
        new ConfigSnapshot(null),
        CancellationToken.None);

    var rootCause = string.IsNullOrWhiteSpace(logs)
        ? null
        : (await logAnalyzer.AnalyzeAsync(logs, CancellationToken.None)).RootCause;

    logger.LogInformation("Plan: {Plan}", string.Join(" | ", plan.Findings));
    if (!string.IsNullOrWhiteSpace(rootCause))
    {
        logger.LogInformation("Root cause: {RootCause}", rootCause);
    }
    llmContext.Clear();
});

var executeCommand = new Command("execute", "Execute the full workflow")
{
    repoOption,
    taskOption,
    providerOption,
    branchOption,
    logsOption,
    maxContextOption,
    llmProviderOption,
    llmModelOption,
    llmApiKeyOption
};

executeCommand.SetHandler(async context =>
{
    var repo = context.ParseResult.GetValueForOption(repoOption);
    var task = context.ParseResult.GetValueForOption(taskOption);
    var provider = context.ParseResult.GetValueForOption(providerOption);
    var branch = context.ParseResult.GetValueForOption(branchOption);
    var logs = context.ParseResult.GetValueForOption(logsOption);
    var maxContext = context.ParseResult.GetValueForOption(maxContextOption);
    var llmProvider = context.ParseResult.GetValueForOption(llmProviderOption);
    var llmModel = context.ParseResult.GetValueForOption(llmModelOption);
    var llmApiKey = context.ParseResult.GetValueForOption(llmApiKeyOption);

    var executor = host.Services.GetRequiredService<IExecutor>();
    var logAnalyzer = host.Services.GetRequiredService<ILogAnalysisService>();
    var llmContext = host.Services.GetRequiredService<ILLMRequestContext>();

    llmContext.Set(new LlmRequestOptions
    {
        Provider = Enum.TryParse<LlmProvider>(llmProvider, true, out var parsedProvider) ? parsedProvider : LlmProvider.Ollama,
        Model = llmModel,
        ApiKey = llmApiKey
    });

    var coreRequest = new AiAgent.Core.Models.AgentRequest
    {
        RepoUrl = repo,
        Task = task,
        Provider = llmProvider,
        Model = llmModel,
        ApiKey = llmApiKey
    };

    var execution = await executor.ExecuteAsync(
        coreRequest,
        maxContext,
        CancellationToken.None);

    var rootCause = string.IsNullOrWhiteSpace(logs)
        ? null
        : (await logAnalyzer.AnalyzeAsync(logs, CancellationToken.None)).RootCause;

    logger.LogInformation("Summary: {Summary}", execution.Response.Summary);
    logger.LogInformation("Plan: {Plan}", string.Join(" | ", execution.Plan));
    logger.LogInformation("Changes: {Count}", execution.Response.CodeChanges.Count);
    logger.LogInformation("Tests: {Count}", execution.Response.TestCases.Count);
    if (!string.IsNullOrWhiteSpace(rootCause))
    {
        logger.LogInformation("Root cause: {RootCause}", rootCause);
    }
    llmContext.Clear();
});

var logsCommand = new Command("logs", "Analyze log content")
{
    logsOption
};

logsCommand.SetHandler(async (string? logs) =>
{
    var logAnalyzer = host.Services.GetRequiredService<ILogAnalysisService>();
    var analysis = await logAnalyzer.AnalyzeAsync(logs ?? string.Empty, CancellationToken.None);

    logger.LogInformation("Root cause: {RootCause}", analysis.RootCause);
    logger.LogInformation("Components: {Components}", string.Join(", ", analysis.AffectedComponents));
    logger.LogInformation("Suggested fix: {SuggestedFix}", analysis.SuggestedFix);
}, logsOption);

var rootCommand = new RootCommand("AI Engineering Workflow Agent CLI")
{
    analyzeCommand,
    executeCommand,
    logsCommand
};

try
{
    return await rootCommand.InvokeAsync(args);
}
finally
{
    Log.CloseAndFlush();
}
