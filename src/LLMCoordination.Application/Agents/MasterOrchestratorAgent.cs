using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class MasterOrchestratorAgent : IAgent
{
    public const string AgentName = "master.orchestrator";

    private readonly ChatOrchestrationService _orchestrationService;

    public MasterOrchestratorAgent(ChatOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest
        {
            ConversationId = context.ConversationId.ToString(),
            Message = context.RawMessage,
            ViewPreference = context.ViewPreference
        };

        var response = await _orchestrationService.ExecuteAsync(request, context, cancellationToken);

        return new AgentResult
        {
            AgentName = AgentName,
            Status = response.Status.Equals("success", StringComparison.OrdinalIgnoreCase)
                ? AgentStatus.Success
                : AgentStatus.Failed,
            OutputJson = System.Text.Json.JsonSerializer.Serialize(response),
            ErrorMessage = response.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ? null : response.Answer
        };
    }
}
