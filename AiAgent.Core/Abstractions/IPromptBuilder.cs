using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IPromptBuilder
{
    string BuildPrompt(IntentType intent, string context, string task);
}
