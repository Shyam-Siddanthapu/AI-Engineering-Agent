namespace AiAgent.Infrastructure.Options;

public sealed class FileApplyOptions
{
    public bool PreviewOnly { get; set; }
    public string BackupDirectory { get; set; } = ".backups";
    public bool CommitChanges { get; set; }
    public string CommitMessage { get; set; } = "Apply AI agent changes";
}
