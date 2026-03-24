using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class PromptBuilder : IPromptBuilder
{
    public string BuildPrompt(IntentType intent, string context, string task)
    {
        var instruction = intent switch
        {
            IntentType.Analysis => "You are a senior software architect. Analyze the repository and explain purpose, architecture, and components.",
            IntentType.TechnicalOverview => "Explain the technical workflow and system design in detail.",
            IntentType.DomainExplanation => "Explain the business/domain logic and use cases of this system.",
            IntentType.GeneralQuestion => "Answer the user's question based on repository context.",
            IntentType.TestGeneration => "Generate test-focused guidance based on the repository context.",
            IntentType.CodeGeneration => "Provide implementation guidance based on the repository context.",
            IntentType.BugFix => "Identify likely fault areas and propose fixes based on the repository context.",
            _ => "Answer the user's question based on repository context."
        };

        return $"""
            {instruction}

            Task:
            {task}

            Context:
            {context}
            """;
    }
}
