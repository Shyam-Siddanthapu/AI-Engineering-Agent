using AiAgent.Core.Abstractions;

namespace AiAgent.Infrastructure.Llm;

public sealed class MockChatCompletionService : IChatCompletionService
{
    public Task<string> GetCompletionAsync(string prompt, string model, string apiKey, CancellationToken cancellationToken = default)
    {
        var normalized = prompt?.ToLowerInvariant() ?? string.Empty;

        var intent = DetectIntent(normalized);
        var summary = Pick(GetSummaryOptions(intent));
        var explanation = Pick(GetExplanationOptions(intent));
        var steps = GetSteps(intent);
        var codeChanges = GetCodeChanges(intent);
        var testCases = GetTestCases(intent);
        var risks = PickMany(GetRiskOptions(intent), 2);
        var suggestions = PickMany(GetSuggestionOptions(intent), 2);

        var response = $$"""
        {
          "intent": "{{intent}}",
          "summary": "{{summary}}",
          "detailedExplanation": "{{explanation}}",
          "steps": [{{string.Join(", ", steps.Select(step => $"\"{Escape(step)}\""))}}],
          "codeChanges": [{{string.Join(", ", codeChanges)}}],
          "testCases": [{{string.Join(", ", testCases.Select(test => $"\"{Escape(test)}\""))}}],
          "risks": [{{string.Join(", ", risks.Select(risk => $"\"{Escape(risk)}\""))}}],
          "suggestions": [{{string.Join(", ", suggestions.Select(suggestion => $"\"{Escape(suggestion)}\""))}}]
        }
        """;

        return Task.FromResult(response);
    }

    private static string DetectIntent(string prompt)
    {
        if (prompt.Contains("workflow", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("architecture", StringComparison.OrdinalIgnoreCase))
        {
            return "TechnicalOverview";
        }

        if (prompt.Contains("fix", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("bug", StringComparison.OrdinalIgnoreCase))
        {
            return "BugFix";
        }

        if (prompt.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "TestGeneration";
        }

        if (prompt.Contains("add", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("feature", StringComparison.OrdinalIgnoreCase))
        {
            return "CodeGeneration";
        }

        if (prompt.Contains("analyze", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("explain", StringComparison.OrdinalIgnoreCase))
        {
            return "Analysis";
        }

        return "Analysis";
    }

    private static IReadOnlyList<string> GetSummaryOptions(string intent) => intent switch
    {
        "TechnicalOverview" =>
        [
            "The solution follows a layered .NET architecture with API, core services, and infrastructure adapters.",
            "This system is organized into API, core orchestration, and infrastructure integrations for LLM and storage."
        ],
        "BugFix" =>
        [
            "The issue is likely caused by missing validation and null guards in the service layer.",
            "A null reference appears to stem from unchecked inputs in the core workflow."
        ],
        "TestGeneration" =>
        [
            "Key workflows need unit tests with edge case coverage to reduce regression risk.",
            "Targeted tests should cover validation, error handling, and service interactions."
        ],
        "CodeGeneration" =>
        [
            "A new feature can be introduced by extending core services and exposing an API endpoint.",
            "The feature implementation should add a service and register it via DI with a new endpoint."
        ],
        _ =>
        [
            "This repository implements a cleanly layered .NET solution with extensible AI workflow components.",
            "The codebase is structured around clean architecture with core orchestration and infrastructure adapters."
        ]
    };

    private static IReadOnlyList<string> GetExplanationOptions(string intent) => intent switch
    {
        "TechnicalOverview" =>
        [
            "Requests enter the API layer, flow through orchestrators, and call infrastructure services for LLM and persistence.",
            "The API delegates to core workflows, which use infrastructure adapters for external services."
        ],
        "BugFix" =>
        [
            "Add null checks and validate inputs in the affected services, then update downstream calls to expect safe values.",
            "Guard the core workflow against null input and ensure dependent services handle empty values."
        ],
        "TestGeneration" =>
        [
            "Cover command handlers, validation rules, and repository interactions using Moq-backed dependencies.",
            "Create unit tests for service boundaries and edge cases to prevent regressions."
        ],
        "CodeGeneration" =>
        [
            "Add a new core service, register it in DI, and expose the functionality through the API layer.",
            "Implement the feature in the core layer and wire it to the API with validation."
        ],
        _ =>
        [
            "The system follows clean architecture with API endpoints calling core orchestrators and infrastructure integrations.",
            "Core logic coordinates workflows while infrastructure services handle external dependencies."
        ]
    };

    private static List<string> GetSteps(string intent) => intent switch
    {
        "TechnicalOverview" => ["Review entry points", "Trace service orchestration", "Summarize infrastructure dependencies"],
        "BugFix" => ["Locate failing code path", "Add defensive checks", "Validate with tests"],
        "TestGeneration" => ["Identify key scenarios", "Create xUnit tests", "Mock external services"],
        "CodeGeneration" => ["Add core service", "Register in DI", "Expose API endpoint"],
        _ => ["Scan repository context", "Summarize components", "Document responsibilities"]
    };

    private static List<string> GetCodeChanges(string intent) => intent switch
    {
        "BugFix" =>
        [
            "{ \"filePath\": \"AiAgent.Core/Services/AgentOrchestrator.cs\", \"originalCode\": \"public async Task ExecuteAsync(...)\", \"modifiedCode\": \"public async Task ExecuteAsync(...) { if (request is null) throw new ArgumentNullException(...); }\" }"
        ],
        "CodeGeneration" =>
        [
            "{ \"filePath\": \"AiAgent.Core/Services/NewFeatureService.cs\", \"originalCode\": \"// new file\", \"modifiedCode\": \"public sealed class NewFeatureService { ... }\" }"
        ],
        _ => new List<string>()
    };

    private static List<string> GetTestCases(string intent) => intent switch
    {
        "TestGeneration" =>
        [
            "Create xUnit tests for request validation",
            "Verify error handling on null inputs",
            "Mock repository calls for core services"
        ],
        _ => new List<string>()
    };

    private static IReadOnlyList<string> GetRiskOptions(string intent) => intent switch
    {
        "BugFix" =>
        [
            "Potential regression if downstream services expect nullable values",
            "Unhandled edge cases may still surface in legacy workflows"
        ],
        "CodeGeneration" =>
        [
            "Feature integration may require schema updates",
            "API contract changes could impact clients"
        ],
        _ =>
        [
            "Mock output is simulated and may differ from real system behavior",
            "Limited context may hide deeper dependencies"
        ]
    };

    private static IReadOnlyList<string> GetSuggestionOptions(string intent) => intent switch
    {
        "TechnicalOverview" =>
        [
            "Document dependencies",
            "Add an architecture diagram",
            "Highlight service boundaries in README"
        ],
        "BugFix" =>
        [
            "Add regression tests",
            "Review edge cases",
            "Introduce guard clauses in core services"
        ],
        "TestGeneration" =>
        [
            "Add integration coverage",
            "Include negative cases",
            "Adopt test data builders for readability"
        ],
        "CodeGeneration" =>
        [
            "Update API contracts",
            "Add validation rules",
            "Expose metrics for the new workflow"
        ],
        _ =>
        [
            "Use a real provider for production-grade analysis",
            "Expand context for higher-fidelity responses"
        ]
    };

    private static string Pick(IReadOnlyList<string> options)
        => options[Random.Shared.Next(options.Count)];

    private static List<string> PickMany(IReadOnlyList<string> options, int count)
    {
        var indices = Enumerable.Range(0, options.Count)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(count, options.Count))
            .ToList();

        return indices.Select(index => options[index]).ToList();
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
