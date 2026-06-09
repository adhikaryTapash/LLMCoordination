using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class ClarificationAgent : IAgent
{
    public const string AgentName = "clarification";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var clarification = nlu?.Clarification ?? new ClarificationInfo();

        if (nlu?.RoutingHints.RequiresPatientResolution == true &&
            nlu.Entities.Patient is null &&
            context.RawMessage.Contains("XXX", StringComparison.OrdinalIgnoreCase))
        {
            clarification = new ClarificationInfo
            {
                Required = true,
                Question = "Which patient did you mean? Please provide a name or MRN."
            };
        }

        context.AgentOutputs[AgentName] = clarification;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = clarification.Required ? AgentStatus.Failed : AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(clarification),
            ErrorMessage = clarification.Required ? clarification.Question : null,
            NextAgentHint = clarification.Required ? null : "tenant.context"
        });
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
