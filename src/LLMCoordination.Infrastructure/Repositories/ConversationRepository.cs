using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId, cancellationToken);

    public async Task<Conversation> CreateAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        conversation.CreatedAt = DateTimeOffset.UtcNow;
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task AddMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default)
    {
        message.CreatedAt = DateTimeOffset.UtcNow;
        _context.ConversationMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
