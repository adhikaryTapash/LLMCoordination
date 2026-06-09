using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class McpExecutionAgent : IAgent
{
    public const string AgentName = "mcp.execution";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var result = new McpToolCallResult
        {
            Status = "success",
            OutputJson = JsonSerializer.Serialize(new { message = "Mock MCP tool execution completed", records = 1 })
        };

        context.AgentOutputs[AgentName] = result;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = result.OutputJson,
            NextAgentHint = "response.composer"
        });
    }
}
