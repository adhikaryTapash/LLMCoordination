using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class QueryPlannerAgent : IAgent
{
    public const string AgentName = "query.planner";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var hints = nlu?.RoutingHints ?? new RoutingHints();

        if (nlu?.Intent.Name.StartsWith("database.", StringComparison.OrdinalIgnoreCase) == true)
        {
            hints.RequiresDatabase = true;
            hints.RequiresSwagger = false;
            hints.SuggestedNextAgent = "database.schema.agent";
        }
        else if (nlu?.Intent.Name.Contains("analytics", StringComparison.OrdinalIgnoreCase) == true)
        {
            hints.RequiresSwagger = false;
            hints.RequiresDatabase = false;
            hints.SuggestedNextAgent = "analytics.agent";
        }
        else
        {
            hints.RequiresSwagger = true;
            hints.SuggestedNextAgent = "endpoint.discovery";
        }

        if (nlu is not null)
        {
            nlu.RoutingHints = hints;
            context.AgentOutputs[NluIntentAgent.AgentName] = nlu;
        }

        context.AgentOutputs[AgentName] = hints;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(hints),
            NextAgentHint = hints.SuggestedNextAgent
        });
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
