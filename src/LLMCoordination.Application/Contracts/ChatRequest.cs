namespace LLMCoordination.Application.Contracts;

public class ChatRequest
{
    public string? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ViewPreference { get; set; }
}
