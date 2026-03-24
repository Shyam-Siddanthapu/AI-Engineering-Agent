using AiAgent.Api.Models;
using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AgentController : ControllerBase
{
    private readonly IPlanner _planner;
    private readonly IRepositoryService _repositoryService;
    private readonly ILogAnalysisService _logAnalysisService;
    private readonly IExecutor _executor;

    public AgentController(
        IPlanner planner,
        IRepositoryService repositoryService,
        ILogAnalysisService logAnalysisService,
        IExecutor executor)
    {
        _planner = planner;
        _repositoryService = repositoryService;
        _logAnalysisService = logAnalysisService;
        _executor = executor;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<AgentResponseDto>> AnalyzeAsync(
        [FromBody] AiAgent.Api.Models.AgentRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var problemRequest = request.ToProblemRequest();
        var repositoryRequest = request.ToRepositoryRequest();
        var repositoryAnalysis = await BuildRepositoryAnalysisAsync(repositoryRequest, cancellationToken);
        var logSnapshot = new LogSnapshot(null);
        var configSnapshot = new ConfigSnapshot(null);

        var plan = await _planner.CreateAnalysisAsync(
            problemRequest,
            repositoryAnalysis,
            logSnapshot,
            configSnapshot,
            cancellationToken);

        var rootCause = await AnalyzeRootCauseAsync(null, cancellationToken);

        var response = new AgentResponseDto(
            Plan: plan.Findings,
            RootCause: rootCause,
            CodeChanges: Array.Empty<CodeChange>(),
            Tests: Array.Empty<string>(),
            Summary: plan.Summary);

        return Ok(response);
    }

    [HttpPost("execute")]
    public async Task<ActionResult<AgentResponseDto>> ExecuteAsync(
        [FromBody] AiAgent.Api.Models.AgentRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var coreRequest = request.ToCoreRequest();
        var execution = await _executor.ExecuteAsync(
            coreRequest,
            12000,
            cancellationToken);

        var rootCause = await AnalyzeRootCauseAsync(null, cancellationToken);

        var response = new AgentResponseDto(
            Plan: execution.Plan,
            RootCause: rootCause,
            CodeChanges: execution.Response.CodeChanges,
            Tests: execution.Response.TestCases,
            Summary: execution.Response.Summary);

        return Ok(response);
    }

    [HttpPost("logs/analyze")]
    public async Task<ActionResult<AgentResponseDto>> AnalyzeLogsAsync(
        [FromBody] LogAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var analysis = await _logAnalysisService.AnalyzeAsync(request.Logs ?? string.Empty, cancellationToken);

        return Ok(new AgentResponseDto(
            Plan: Array.Empty<string>(),
            RootCause: analysis.RootCause,
            CodeChanges: Array.Empty<CodeChange>(),
            Tests: Array.Empty<string>(),
            Summary: analysis.SuggestedFix));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<AgentExecutionDto>>> GetHistoryAsync(
        [FromServices] IAgentExecutionRepository repository,
        CancellationToken cancellationToken)
    {
        var executions = await repository.GetAllAsync(cancellationToken);
        var result = executions
            .Select(execution => new AgentExecutionDto(
                execution.Id,
                execution.RepoUrl,
                execution.Task,
                execution.Provider,
                execution.Model,
                execution.CreatedAt))
            .ToList();

        return Ok(result);
    }

    [HttpGet("history/{id:guid}")]
    public async Task<ActionResult<AgentExecutionDetailDto>> GetHistoryDetailAsync(
        Guid id,
        [FromServices] IAgentExecutionRepository repository,
        CancellationToken cancellationToken)
    {
        var execution = await repository.GetByIdAsync(id, cancellationToken);
        if (execution is null)
        {
            return NotFound();
        }

        return Ok(new AgentExecutionDetailDto(
            execution.Id,
            execution.RepoUrl,
            execution.Task,
            execution.Provider,
            execution.Model,
            execution.Result,
            execution.CreatedAt));
    }

    private async Task<RepositoryAnalysis> BuildRepositoryAnalysisAsync(
        RepositoryRequest repositoryRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(repositoryRequest.RepositoryUrl))
        {
            return new RepositoryAnalysis("No repository provided.", Array.Empty<string>());
        }

        var files = await _repositoryService.GetFileListAsync(repositoryRequest, cancellationToken);
        var summary = files.Count == 0
            ? "Repository returned no files."
            : $"Repository returned {files.Count} files.";

        return new RepositoryAnalysis(summary, files);
    }

    private async Task<string?> AnalyzeRootCauseAsync(string? logs, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(logs))
        {
            return null;
        }

        var analysis = await _logAnalysisService.AnalyzeAsync(logs, cancellationToken);
        return analysis.RootCause;
    }

    public sealed record LogAnalysisRequest(string? Logs);

    public sealed record AgentResponseDto(
        IReadOnlyList<string> Plan,
        string? RootCause,
        IReadOnlyList<CodeChange> CodeChanges,
        IReadOnlyList<string> Tests,
        string Summary);

    public sealed record AgentExecutionDto(
        Guid Id,
        string RepoUrl,
        string Task,
        string Provider,
        string? Model,
        DateTimeOffset CreatedAt);

    public sealed record AgentExecutionDetailDto(
        Guid Id,
        string RepoUrl,
        string Task,
        string Provider,
        string? Model,
        string Result,
        DateTimeOffset CreatedAt);
}
