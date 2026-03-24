namespace AiAgent.Infrastructure.Options;

public sealed class GitHubOptions
{
    public string BaseUrl { get; set; } = "https://api.github.com";
    public string? Token { get; set; }
}
