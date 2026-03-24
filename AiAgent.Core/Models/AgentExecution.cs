namespace AiAgent.Core.Models;

public sealed class AgentExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RepoUrl { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string Result { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
