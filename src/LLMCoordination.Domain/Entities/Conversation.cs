namespace LLMCoordination.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public ICollection<ConversationMessage> Messages { get; set; } = [];
}
