namespace LLMCoordination.Application.Contracts;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ConversationId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string? SkillName { get; set; }
    public string? ToolName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ExecutionMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
