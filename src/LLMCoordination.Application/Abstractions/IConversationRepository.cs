using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid conversationId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
}
