namespace LLMCoordination.Domain.Entities;

public class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public string? ResponseJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
