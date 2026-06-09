using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class ApprovalAgent : IAgent
{
    public const string AgentName = "approval";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var action = nlu?.Intent.Action ?? "read";
        var requiresApproval = action is "delete" or "create" or "update";

        var response = new ApprovalResponse
        {
            Required = requiresApproval,
            Granted = !requiresApproval || context.ApprovalGranted,
            PendingAction = requiresApproval ? action : null,
            Message = requiresApproval && !context.ApprovalGranted
                ? $"Approval required for {action} operation."
                : null
        };

        context.AgentOutputs[AgentName] = response;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = response.Granted ? AgentStatus.Success : AgentStatus.Failed,
            OutputJson = JsonSerializer.Serialize(response),
            ErrorMessage = response.Granted ? null : response.Message
        });
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
