namespace LLMCoordination.Application.Contracts;

public class ApprovalRequest
{
    public string? ConversationId { get; set; }
    public string ApprovalToken { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Approved { get; set; }
}
