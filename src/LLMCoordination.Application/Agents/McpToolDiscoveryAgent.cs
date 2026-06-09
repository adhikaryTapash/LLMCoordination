using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class McpToolDiscoveryAgent : IAgent
{
    public const string AgentName = "mcp.tool.discovery";

    private readonly IMcpRepository _mcpRepository;

    public McpToolDiscoveryAgent(IMcpRepository mcpRepository)
    {
        _mcpRepository = mcpRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var tools = await _mcpRepository.GetToolsAsync(context.TenantId, cancellationToken);
        var message = context.RawMessage.ToLowerInvariant();

        var matched = tools
            .Where(t => message.Contains(t.ToolName, StringComparison.OrdinalIgnoreCase) ||
                        message.Contains(t.Description, StringComparison.OrdinalIgnoreCase))
            .Select(t => new McpToolDto
            {
                Id = t.Id,
                ToolName = t.ToolName,
                Description = t.Description,
                InputSchemaJson = t.InputSchemaJson
            })
            .ToList();

        if (matched.Count == 0 && tools.Count > 0)
        {
            matched.Add(new McpToolDto
            {
                Id = tools[0].Id,
                ToolName = tools[0].ToolName,
                Description = tools[0].Description,
                InputSchemaJson = tools[0].InputSchemaJson
            });
        }

        context.AgentOutputs[AgentName] = matched;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(matched),
            NextAgentHint = "mcp.execution"
        };
    }
}
