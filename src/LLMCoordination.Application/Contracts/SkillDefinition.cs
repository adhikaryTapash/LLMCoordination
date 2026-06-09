namespace LLMCoordination.Application.Contracts;

public class SkillDefinition
{
    public string SkillName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionType { get; set; } = "Read";
}
