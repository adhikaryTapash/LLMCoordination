using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class AnalyticsAgent : IAgent
{
    public const string AgentName = "analytics";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var insight = new
        {
            summary = "Mock analytics insight based on tenant data trends.",
            trend = "up",
            comparisonPeriod = "last_30_days",
            metrics = new { patientVisits = 120, appointments = 85, cancellations = 5 }
        };

        context.AgentOutputs[AgentName] = insight;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(insight),
            NextAgentHint = "response.composer"
        });
    }
}
