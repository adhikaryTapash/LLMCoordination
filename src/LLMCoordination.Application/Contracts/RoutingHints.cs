namespace LLMCoordination.Application.Contracts;

public class RoutingHints
{
    public bool RequiresPatientResolution { get; set; }
    public bool RequiresDatabase { get; set; }
    public bool RequiresSwagger { get; set; }
    public bool RequiresMcpTool { get; set; }
    public string? SuggestedSkill { get; set; }
    public string? SuggestedNextAgent { get; set; }
}
