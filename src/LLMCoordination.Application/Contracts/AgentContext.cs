namespace LLMCoordination.Application.Contracts;

public class AgentContext
{
    public Guid ConversationId { get; set; }
    public string TurnId { get; set; } = Guid.NewGuid().ToString("N");
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string UserRole { get; set; } = string.Empty;
    public string RawMessage { get; set; } = string.Empty;
    public string ViewPreference { get; set; } = "text";
    public bool ApprovalGranted { get; set; }
    public Dictionary<string, object> AgentOutputs { get; } = new();
}
