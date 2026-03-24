using AiAgent.Core.Models;

namespace AiAgent.Core.Abstractions;

public interface IIntentClassifier
{
    IntentType Classify(string task);
}
