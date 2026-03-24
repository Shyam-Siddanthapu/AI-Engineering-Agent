using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AiAgent.Core.Services; // Added for LLM request context usage

namespace AiAgent.Api.Pages;

public sealed class ResultsModel : PageModel
{
    private readonly IPlanner _planner;
    private readonly IRepositoryService _repositoryService;
    private readonly ILogAnalysisService _logAnalysisService;
    private readonly IExecutor _executor;
    private readonly ILLMRequestContext _llmRequestContext;

    public ResultsModel(
        IPlanner planner,
        IRepositoryService repositoryService,
        ILogAnalysisService logAnalysisService,
        IExecutor executor,
        ILLMRequestContext llmRequestContext)
    {
        _planner = planner;
        _repositoryService = repositoryService;
        _logAnalysisService = logAnalysisService;
        _executor = executor;
        _llmRequestContext = llmRequestContext;
    }

    [BindProperty]
    public string RepositoryUrl { get; set; } = string.Empty;

    [BindProperty]
    public string Task { get; set; } = string.Empty;

    [BindProperty]
    public string? Logs { get; set; }

    [BindProperty]
    public LlmProvider Provider { get; set; } = LlmProvider.Ollama;

    [BindProperty]
    public string? Model { get; set; }

    [BindProperty]
    public string? ApiKey { get; set; }

    [BindProperty]
    public int MaxContextCharacters { get; set; } = 12000;

    public IReadOnlyList<string> Plan { get; private set; } = Array.Empty<string>();
    public IReadOnlyList<CodeChange> CodeChanges { get; private set; } = Array.Empty<CodeChange>();
    public IReadOnlyList<string> Tests { get; private set; } = Array.Empty<string>();
    public IReadOnlyList<string> Risks { get; private set; } = Array.Empty<string>();
    public IReadOnlyList<string> Suggestions { get; private set; } = Array.Empty<string>();
    public string Summary { get; private set; } = string.Empty;
    public string DetailedExplanation { get; private set; } = string.Empty;
    public string? RootCause { get; private set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(RepositoryUrl) || string.IsNullOrWhiteSpace(Task))
        {
            ModelState.AddModelError(string.Empty, "Repository URL and task are required.");
            return Page();
        }

        var coreRequest = new Core.Models.AgentRequest
        {
            RepoUrl = RepositoryUrl,
            Task = Task,
            Provider = Provider.ToString(),
            Model = Model,
            ApiKey = ApiKey
        };

        _llmRequestContext.Set(new LlmRequestOptions
        {
            Provider = Provider,
            Model = Model,
            ApiKey = ApiKey
        });

        var execution = await _executor.ExecuteAsync(
            coreRequest,
            MaxContextCharacters,
            cancellationToken);

        Plan = execution.Plan;
        CodeChanges = execution.Response.CodeChanges;
        Tests = execution.Response.TestCases;
        Risks = execution.Response.Risks;
        Suggestions = execution.Response.Suggestions;
        Summary = execution.Response.Summary;
        DetailedExplanation = execution.Response.DetailedExplanation;

        if (!string.IsNullOrWhiteSpace(Logs))
        {
            var analysis = await _logAnalysisService.AnalyzeAsync(Logs, cancellationToken);
            RootCause = analysis.RootCause;
        }

        _llmRequestContext.Clear();
        return Page();
    }
}
