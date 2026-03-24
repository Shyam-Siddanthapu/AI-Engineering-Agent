namespace AiAgent.Infrastructure.Options;

public sealed class AzureDevOpsOptions
{
    public string BaseUrl { get; set; } = "https://dev.azure.com";
    public string? Organization { get; set; }
    public string? Project { get; set; }
    public string? Token { get; set; }
}
