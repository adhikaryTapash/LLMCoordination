namespace LLMCoordination.Application.Contracts;

public class ChatResponse
{
    public string ConversationId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
    public string ViewType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public object? Data { get; set; }
}
