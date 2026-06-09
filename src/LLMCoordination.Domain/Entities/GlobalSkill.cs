using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Domain.Entities;

public class GlobalSkill
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public DomainType Domain { get; set; }
    public SkillCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<GlobalSkillIntentExample> IntentExamples { get; set; } = [];
    public ICollection<GlobalSkillPrompt> Prompts { get; set; } = [];
    public ICollection<GlobalSkillResponseSchema> ResponseSchemas { get; set; } = [];
    public ICollection<TenantSkillEndpointMapping> EndpointMappings { get; set; } = [];
    public ICollection<TenantSkillMcpMapping> McpMappings { get; set; } = [];
}
