using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.Llm;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiAgent.Infrastructure.Services;

public sealed class Executor : IExecutor
{
    private readonly IIntentClassifier _intentClassifier;
    private readonly ICodeContextBuilder _codeContextBuilder;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ChatCompletionFactory _chatCompletionFactory;
    private readonly FileApplyService _fileApplyService;
    private readonly RepoWorkspaceManager _workspaceManager;
    private readonly DiffService _diffService;
    private readonly IAgentExecutionRepository _executionRepository;
    private readonly ILogger<Executor> _logger;

    public Executor(
        IIntentClassifier intentClassifier,
        ICodeContextBuilder codeContextBuilder,
        IPromptBuilder promptBuilder,
        ChatCompletionFactory chatCompletionFactory,
        FileApplyService fileApplyService,
        RepoWorkspaceManager workspaceManager,
        DiffService diffService,
        IAgentExecutionRepository executionRepository,
        ILogger<Executor> logger)
    {
        _intentClassifier = intentClassifier;
        _codeContextBuilder = codeContextBuilder;
        _promptBuilder = promptBuilder;
        _chatCompletionFactory = chatCompletionFactory;
        _fileApplyService = fileApplyService;
        _workspaceManager = workspaceManager;
        _diffService = diffService;
        _executionRepository = executionRepository;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(
        AiAgent.Core.Models.AgentRequest request,
        int maxContextCharacters,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var intent = _intentClassifier.Classify(request.Task);

        var repository = new RepositoryRequest
        {
            RepositoryUrl = request.RepoUrl
        };

        string context = string.Empty;
        try
        {
            context = await _codeContextBuilder.BuildContextAsync(
                repository,
                request.Task,
                maxContextCharacters,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Context builder failed.");
        }

        var promptTemplate = _promptBuilder.BuildPrompt(intent, context, request.Task);
        var prompt = promptTemplate
            .Replace("{{$task}}", request.Task, StringComparison.Ordinal)
            .Replace("{{$context}}", context, StringComparison.Ordinal);
        var chatService = _chatCompletionFactory.GetService(request.Provider);

        _logger.LogInformation("LLM provider: {Provider}", request.Provider);
        _logger.LogInformation("LLM model: {Model}", string.IsNullOrWhiteSpace(request.Model) ? "default" : request.Model);

        _logger.LogInformation("PROMPT: {Prompt}", prompt);

        string responseText = string.Empty;
        try
        {
            responseText = await ExecuteWithRetriesAsync(
                chatService,
                prompt,
                request.Model ?? string.Empty,
                request.ApiKey ?? string.Empty,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM call failed after retries.");
        }

        _logger.LogInformation("RESPONSE: {Response}", responseText);
        var response = ParseResponse(responseText, intent, request.Task);

        try
        {
            if (response.CodeChanges.Count > 0)
            {
                var workspacePath = await _workspaceManager.GetOrCreateWorkspace(request.RepoUrl);
                var diffOutput = new List<CodeChange>();

                foreach (var change in response.CodeChanges)
                {
                    var originalPath = Path.Combine(workspacePath, change.FilePath);
                    var original = File.Exists(originalPath)
                        ? await File.ReadAllTextAsync(originalPath, cancellationToken)
                        : string.Empty;

                    var diff = _diffService.CreateDiff(original, change.ModifiedCode);
                    diffOutput.Add(new CodeChange
                    {
                        FilePath = change.FilePath,
                        OriginalCode = original,
                        ModifiedCode = string.Join(Environment.NewLine, diff.Lines.Select(line => $"{line.Prefix} {line.Text}"))
                    });
                }

                response = response with { CodeChanges = diffOutput };

                await _fileApplyService.ApplyChanges(workspacePath, response.CodeChanges, request.ExecutionMode, request.Provider, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Applying changes failed.");
        }

        try
        {
            var execution = new AgentExecution
            {
                RepoUrl = request.RepoUrl,
                Task = request.Task,
                Provider = request.Provider,
                Model = request.Model,
                Result = responseText,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _executionRepository.AddAsync(execution, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Persisting execution failed.");
        }

        return new ExecutionResult(response.Steps, response);
    }

    private async Task<string> ExecuteWithRetriesAsync(
        IChatCompletionService chatService,
        string prompt,
        string model,
        string apiKey,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 2;
        var attempt = 0;
        Exception? lastError = null;

        while (attempt <= maxRetries)
        {
            try
            {
                return await chatService.GetCompletionAsync(prompt, model, apiKey, cancellationToken);
            }
            catch (Exception ex)
            {
                lastError = ex;
                _logger.LogWarning(ex, "LLM call failed on attempt {Attempt}.", attempt + 1);
                attempt++;
            }
        }

        throw new InvalidOperationException("LLM call failed after retries.", lastError);
    }

    private AgentResponse ParseResponse(string responseText, IntentType intent, string task)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return new AgentResponse
            {
                Intent = intent.ToString(),
                Summary = string.Empty,
                DetailedExplanation = string.Empty,
                Steps = new List<string> { task }
            };
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<AgentResponse>(responseText, options);
            if (parsed is not null)
            {
                return parsed with
                {
                    Intent = string.IsNullOrWhiteSpace(parsed.Intent) ? intent.ToString() : parsed.Intent,
                    Steps = parsed.Steps.Count > 0 ? parsed.Steps : new List<string> { task }
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON returned from LLM.");
        }

        return new AgentResponse
        {
            Intent = intent.ToString(),
            Summary = responseText,
            DetailedExplanation = string.Empty,
            Steps = new List<string> { task }
        };
    }

}
