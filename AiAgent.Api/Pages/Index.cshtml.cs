using AiAgent.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AiAgent.Api.Pages;

public sealed class IndexModel : PageModel
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public LlmProvider Provider { get; set; } = LlmProvider.Ollama;
    public string? Model { get; set; }
    public string? ApiKey { get; set; }

    public void OnGet()
    {
    }
}
