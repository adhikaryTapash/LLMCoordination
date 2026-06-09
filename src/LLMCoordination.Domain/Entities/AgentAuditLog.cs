using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Domain.Entities;

public class AgentAuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid? TurnId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string? SkillName { get; set; }
    public ToolType? ToolType { get; set; }
    public string? ToolName { get; set; }
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public AgentStatus Status { get; set; }
    public decimal? Cost { get; set; }
    public int? ExecutionMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
