using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class SuperPromptBuilder : IPromptBuilder
{
    public string BuildPrompt(IntentType intent, string context, string task)
    {
        var template = """
            SYSTEM ROLE:
            "You are a senior software architect with 20+ years of experience in .NET and distributed systems."

            INSTRUCTIONS:
            * Always analyze the repository context before answering
            * Be precise and structured
            * If code changes are needed, provide them clearly
            * If explanation is required, give step-by-step breakdown
            * Avoid hallucination — use only provided context

            OUTPUT FORMAT (STRICT):
            Return JSON with:
            {{
              "intent": "{0}",
              "summary": "",
              "detailedExplanation": "",
              "steps": [],
              "codeChanges": [],
              "testCases": [],
              "risks": [],
              "suggestions": []
            }}

            PROMPT TEMPLATE:

            User Task:
            {{{{$task}}}}

            Repository Context:
            {{{{$context}}}}

            Generate response following the strict JSON format.
            """;

        return string.Format(System.Globalization.CultureInfo.InvariantCulture, template, intent);
    }
}
