namespace LLMCoordination.Domain.Entities;

public class TenantSkillEndpointMapping
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SkillId { get; set; }
    public Guid EndpointId { get; set; }
    public double ConfidenceScore { get; set; }
    public bool Enabled { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public GlobalSkill Skill { get; set; } = null!;
    public TenantSwaggerEndpoint Endpoint { get; set; } = null!;
}
