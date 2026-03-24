namespace AiAgent.Infrastructure.Options;

public sealed class AzureOpenAiOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiVersion { get; set; } = "2024-02-15-preview";
}
