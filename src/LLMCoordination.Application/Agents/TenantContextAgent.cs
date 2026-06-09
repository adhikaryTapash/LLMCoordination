using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class TenantContextAgent : IAgent
{
    public const string AgentName = "tenant.context";

    private readonly ITenantRepository _tenantRepository;
    private readonly ISwaggerRepository _swaggerRepository;
    private readonly IMcpRepository _mcpRepository;
    private readonly IDatabaseRepository _databaseRepository;

    public TenantContextAgent(
        ITenantRepository tenantRepository,
        ISwaggerRepository swaggerRepository,
        IMcpRepository mcpRepository,
        IDatabaseRepository databaseRepository)
    {
        _tenantRepository = tenantRepository;
        _swaggerRepository = swaggerRepository;
        _mcpRepository = mcpRepository;
        _databaseRepository = databaseRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(context.TenantId, cancellationToken);
        if (tenant is null)
        {
            return new AgentResult
            {
                AgentName = AgentName,
                Status = AgentStatus.Failed,
                ErrorMessage = $"Tenant {context.TenantId} not found."
            };
        }

        var swaggerDocs = await _swaggerRepository.GetDocumentsAsync(context.TenantId, cancellationToken);
        var mcpServers = await _mcpRepository.GetServersAsync(context.TenantId, cancellationToken);
        var dbConnections = await _databaseRepository.GetConnectionsAsync(context.TenantId, cancellationToken);
        var permissions = await _tenantRepository.GetPermissionRulesAsync(context.TenantId, cancellationToken);

        var output = new
        {
            tenant.Id,
            tenant.Name,
            tenant.DomainType,
            tenant.Status,
            SwaggerDocumentCount = swaggerDocs.Count,
            McpServerCount = mcpServers.Count,
            DatabaseConnectionCount = dbConnections.Count,
            PermissionRuleCount = permissions.Count
        };

        context.AgentOutputs[AgentName] = output;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(output),
            NextAgentHint = "security.rbac"
        };
    }
}
