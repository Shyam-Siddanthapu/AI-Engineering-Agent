namespace AiAgent.Core.Models;

public sealed record ValidationResult(bool Succeeded, IReadOnlyList<string> ImpactNotes);
