namespace LLMCoordination.Domain.Entities;

public class TenantSkillMcpMapping
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SkillId { get; set; }
    public Guid McpToolId { get; set; }
    public double ConfidenceScore { get; set; }
    public bool Enabled { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public GlobalSkill Skill { get; set; } = null!;
    public TenantMcpTool McpTool { get; set; } = null!;
}
