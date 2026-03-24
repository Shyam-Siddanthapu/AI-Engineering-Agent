using AiAgent.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace AiAgent.Api.Models;

public sealed class AgentRequest : IValidatableObject
{
    [Required]
    [Url]
    public string RepoUrl { get; set; } = string.Empty;

    [Required]
    public string Task { get; set; } = string.Empty;

    [Required]
    public string Provider { get; set; } = "Ollama";

    public string? Model { get; set; }

    public string? ApiKey { get; set; }

    public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Preview;

    public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(Provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Provider, "Groq", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                yield return new System.ComponentModel.DataAnnotations.ValidationResult(
                    "ApiKey is required for Azure OpenAI and Groq providers.",
                    new[] { nameof(ApiKey) });
            }
        }
    }

    public ProblemRequest ToProblemRequest() => new()
    {
        Title = Task,
        Description = Task,
        Type = ProblemType.Incident,
        RepositoryUrl = RepoUrl,
        ApplyChanges = false
    };

    public RepositoryRequest ToRepositoryRequest() => new()
    {
        RepositoryUrl = RepoUrl
    };

    public global::AiAgent.Core.Models.AgentRequest ToCoreRequest() => new()
    {
        RepoUrl = RepoUrl,
        Task = Task,
        Provider = Provider,
        Model = Model,
        ApiKey = ApiKey,
        ExecutionMode = ExecutionMode
    };

    public LlmRequestOptions ToLlmOptions()
    {
        var provider = Enum.TryParse<LlmProvider>(Provider, true, out var parsed)
            ? parsed
            : LlmProvider.Ollama;

        return new LlmRequestOptions
        {
            Provider = provider,
            Model = Model,
            ApiKey = ApiKey
        };
    }
}
