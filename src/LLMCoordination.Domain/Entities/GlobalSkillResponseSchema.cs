namespace LLMCoordination.Domain.Entities;

public class GlobalSkillResponseSchema
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string ResponseSchemaJson { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool Active { get; set; } = true;

    public GlobalSkill Skill { get; set; } = null!;
}
