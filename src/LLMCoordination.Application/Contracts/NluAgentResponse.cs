namespace LLMCoordination.Application.Contracts;

public class NluAgentResponse
{
    public string AgentId { get; set; } = "nlu.intent";
    public string Status { get; set; } = "success";
    public IntentResult Intent { get; set; } = new();
    public EntityResult Entities { get; set; } = new();
    public RoutingHints RoutingHints { get; set; } = new();
    public ClarificationInfo Clarification { get; set; } = new();
}
