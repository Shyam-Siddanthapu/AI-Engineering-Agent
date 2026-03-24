using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class IntentClassifier : IIntentClassifier
{
    private static readonly string[] AnalysisKeywords = ["analyze", "analysis", "explain", "summary", "overview", "architecture"];
    private static readonly string[] TechnicalOverviewKeywords = ["technical overview", "system overview", "components", "architecture", "design"];
    private static readonly string[] DomainKeywords = ["domain", "business", "use case", "workflow", "process"];
    private static readonly string[] CodeGenKeywords = ["generate", "create", "implement", "code", "refactor", "add"];
    private static readonly string[] TestKeywords = ["test", "xunit", "unit", "integration", "coverage", "mock"];
    private static readonly string[] BugKeywords = ["bug", "fix", "issue", "defect", "error", "crash", "broken"];

    public IntentType Classify(string task)
    {
        if (string.IsNullOrWhiteSpace(task))
        {
            return IntentType.GeneralQuestion;
        }

        var normalized = task.ToLowerInvariant();

        if (ContainsAny(normalized, TestKeywords))
        {
            return IntentType.TestGeneration;
        }

        if (ContainsAny(normalized, BugKeywords))
        {
            return IntentType.BugFix;
        }

        if (ContainsAny(normalized, CodeGenKeywords))
        {
            return IntentType.CodeGeneration;
        }

        if (ContainsAny(normalized, TechnicalOverviewKeywords))
        {
            return IntentType.TechnicalOverview;
        }

        if (ContainsAny(normalized, DomainKeywords))
        {
            return IntentType.DomainExplanation;
        }

        if (ContainsAny(normalized, AnalysisKeywords))
        {
            return IntentType.Analysis;
        }

        return IntentType.GeneralQuestion;
    }

    private static bool ContainsAny(string input, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
