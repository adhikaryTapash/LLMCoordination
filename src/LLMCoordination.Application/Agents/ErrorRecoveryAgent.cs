using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class ErrorRecoveryAgent : IAgent
{
    public const string AgentName = "error.recovery";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var recovery = new
        {
            recovered = true,
            suggestion = "Please rephrase your question or provide missing details.",
            retryAllowed = true
        };

        context.AgentOutputs[AgentName] = recovery;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(recovery)
        });
    }
}
