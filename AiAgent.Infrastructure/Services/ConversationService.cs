using AiAgent.Core.Models;
using AiAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiAgent.Infrastructure.Services;

public sealed class ConversationService
{
    private readonly AgentExecutionDbContext _dbContext;

    public ConversationService(AgentExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation?> GetConversation(Guid conversationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(conversation => conversation.Id == conversationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetConversations(CancellationToken cancellationToken)
    {
        return await _dbContext.Conversations
            .AsNoTracking()
            .OrderByDescending(conversation => conversation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetMessages(Guid conversationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Messages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> CreateConversation(string title, string repoUrl, CancellationToken cancellationToken)
    {
        var conversation = new Conversation
        {
            Title = title,
            RepoUrl = repoUrl,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return conversation;
    }
}
