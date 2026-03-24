using AiAgent.Core.Models;
using AiAgent.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AiAgent.Api.Pages;

public sealed class IndexModel : PageModel
{
    private readonly ConversationService _conversationService;

    public IndexModel(ConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [BindProperty]
    public string RepositoryUrl { get; set; } = string.Empty;

    [BindProperty]
    public string Task { get; set; } = string.Empty;

    [BindProperty]
    public LlmProvider Provider { get; set; } = LlmProvider.Ollama;

    [BindProperty]
    public string? Model { get; set; }

    [BindProperty]
    public string? ApiKey { get; set; }

    [BindProperty]
    public Guid? ConversationId { get; set; }

    public async Task OnGetAsync(Guid? conversationId, CancellationToken cancellationToken)
    {
        ConversationId = conversationId;
        if (conversationId.HasValue)
        {
            var conversation = await _conversationService.GetConversation(conversationId.Value, cancellationToken);
            if (conversation is not null)
            {
                RepositoryUrl = conversation.RepoUrl;
                Task = string.Empty;
            }
        }
    }
}
