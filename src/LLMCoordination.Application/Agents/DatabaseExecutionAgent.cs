using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class DatabaseExecutionAgent : IAgent
{
    public const string AgentName = "database.execution";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var mockRows = new[]
        {
            new Dictionary<string, object> { ["count"] = 3 }
        };

        var result = new SqlExecutionResult
        {
            Status = "success",
            RawJson = JsonSerializer.Serialize(mockRows),
            RowCount = mockRows.Length
        };

        context.AgentOutputs[AgentName] = result;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = result.RawJson,
            NextAgentHint = "response.composer"
        });
    }
}
