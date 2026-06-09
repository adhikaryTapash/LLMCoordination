namespace LLMCoordination.Application.Contracts;

public class SkillDetailDto
{
    public string SkillName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public IReadOnlyList<string> IntentExamples { get; set; } = [];
}
