using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Services;

public class McpMetadataService
{
    private readonly IMcpRepository _mcpRepository;

    public McpMetadataService(IMcpRepository mcpRepository)
    {
        _mcpRepository = mcpRepository;
    }

    public async Task<McpServerDto> RegisterServerAsync(
        Guid tenantId,
        RegisterMcpServerRequest request,
        CancellationToken cancellationToken = default)
    {
        var server = new TenantMcpServer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            BaseUrl = request.BaseUrl,
            EncryptedCredentials = request.CredentialValue,
            Status = "Active"
        };

        await _mcpRepository.AddServerAsync(server, cancellationToken);

        var tools = new List<TenantMcpTool>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                McpServerId = server.Id,
                ToolName = "search_records",
                Description = "Search tenant records via MCP",
                InputSchemaJson = """{"type":"object","properties":{"query":{"type":"string"}}}""",
                OutputSchemaJson = """{"type":"object"}"""
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                McpServerId = server.Id,
                ToolName = "get_patient_summary",
                Description = "Get patient summary via MCP",
                InputSchemaJson = """{"type":"object","properties":{"patientId":{"type":"string"}}}""",
                OutputSchemaJson = """{"type":"object"}"""
            }
        };

        await _mcpRepository.AddToolsAsync(tools, cancellationToken);

        return new McpServerDto
        {
            Id = server.Id,
            Name = server.Name,
            BaseUrl = server.BaseUrl,
            Status = server.Status,
            ToolCount = tools.Count
        };
    }

    public async Task<IReadOnlyList<McpToolDto>> GetToolsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tools = await _mcpRepository.GetToolsAsync(tenantId, cancellationToken);
        return tools.Select(t => new McpToolDto
        {
            Id = t.Id,
            ToolName = t.ToolName,
            Description = t.Description,
            InputSchemaJson = t.InputSchemaJson
        }).ToList();
    }

    public Task<McpToolCallResult> ExecuteToolAsync(
        McpToolCallRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new McpToolCallResult
        {
            Status = "success",
            OutputJson = $$"""{"tool":"{{request.ToolName}}","mock":true}"""
        });
    }
}
