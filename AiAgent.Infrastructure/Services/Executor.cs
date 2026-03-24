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
    private readonly AiAgent.Infrastructure.Persistence.AgentExecutionDbContext _dbContext;
    private readonly ConversationService _conversationService;
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
        AiAgent.Infrastructure.Persistence.AgentExecutionDbContext dbContext,
        ConversationService conversationService,
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
        _dbContext = dbContext;
        _conversationService = conversationService;
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

        Conversation? conversation = null;
        IReadOnlyList<Message> history = Array.Empty<Message>();
        try
        {
            if (request.ConversationId.HasValue)
            {
                conversation = await _conversationService.GetConversation(request.ConversationId.Value, cancellationToken);
            }

            if (conversation is null)
            {
                conversation = await _conversationService.CreateConversation(request.Task, request.RepoUrl, cancellationToken);
            }

            history = await _conversationService.GetMessages(conversation.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversation initialization failed.");
        }

        var promptTemplate = _promptBuilder.BuildPrompt(intent, context, request.Task);
        var historyPrompt = BuildHistoryPrompt(history, request.Task);
        var prompt = promptTemplate
            .Replace("{{$task}}", request.Task, StringComparison.Ordinal)
            .Replace("{{$context}}", context, StringComparison.Ordinal)
            + Environment.NewLine
            + Environment.NewLine
            + historyPrompt;
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
                ConversationId = conversation?.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _executionRepository.AddAsync(execution, cancellationToken);

            if (conversation is not null)
            {
                _dbContext.Messages.Add(new Message
                {
                    ConversationId = conversation.Id,
                    Role = "user",
                    Content = request.Task,
                    CreatedAt = DateTime.UtcNow
                });

                _dbContext.Messages.Add(new Message
                {
                    ConversationId = conversation.Id,
                    Role = "assistant",
                    Content = responseText,
                    CreatedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
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

    private static string BuildHistoryPrompt(IReadOnlyList<Message> history, string currentTask)
    {
        if (history.Count == 0)
        {
            return $"Conversation history:{Environment.NewLine}User: {currentTask}{Environment.NewLine}{Environment.NewLine}Now respond to latest request";
        }

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Conversation history:");
        foreach (var message in history)
        {
            var role = string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase) ? "Assistant" : "User";
            builder.AppendLine($"{role}: {message.Content}");
        }

        builder.AppendLine($"User: {currentTask}");
        builder.AppendLine();
        builder.AppendLine("Now respond to latest request");
        return builder.ToString();
    }

}
