using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Contracts;

public class AgentResult
{
    public string AgentName { get; set; } = string.Empty;
    public AgentStatus Status { get; set; } = AgentStatus.Success;
    public string OutputJson { get; set; } = string.Empty;
    public string? NextAgentHint { get; set; }
    public string? ErrorMessage { get; set; }
}
