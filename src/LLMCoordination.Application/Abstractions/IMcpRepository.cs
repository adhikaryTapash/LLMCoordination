using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface IMcpRepository
{
    Task<TenantMcpServer> AddServerAsync(TenantMcpServer server, CancellationToken cancellationToken = default);
    Task AddToolsAsync(IEnumerable<TenantMcpTool> tools, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantMcpServer>> GetServersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantMcpTool>> GetToolsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantMcpTool?> GetToolByNameAsync(Guid tenantId, string toolName, CancellationToken cancellationToken = default);
    Task<TenantMcpServer?> GetServerByIdAsync(Guid tenantId, Guid serverId, CancellationToken cancellationToken = default);
    Task<TenantMcpTool?> GetToolByIdAsync(Guid tenantId, Guid toolId, CancellationToken cancellationToken = default);
}
