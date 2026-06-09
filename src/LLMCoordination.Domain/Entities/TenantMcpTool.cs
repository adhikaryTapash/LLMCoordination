namespace LLMCoordination.Domain.Entities;

public class TenantMcpTool
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid McpServerId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputSchemaJson { get; set; } = string.Empty;
    public string OutputSchemaJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TenantMcpServer McpServer { get; set; } = null!;
    public ICollection<TenantSkillMcpMapping> SkillMappings { get; set; } = [];
}
