namespace LLMCoordination.Application.Contracts;

public class SkillRouterResult
{
    public string SelectedSkill { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
