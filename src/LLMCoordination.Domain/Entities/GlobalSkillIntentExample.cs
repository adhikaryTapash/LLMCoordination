namespace LLMCoordination.Domain.Entities;

public class GlobalSkillIntentExample
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string ExampleText { get; set; } = string.Empty;

    public GlobalSkill Skill { get; set; } = null!;
}
