using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class McpRepository : IMcpRepository
{
    private readonly AppDbContext _context;

    public McpRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TenantMcpServer> AddServerAsync(
        TenantMcpServer server,
        CancellationToken cancellationToken = default)
    {
        server.CreatedAt = DateTimeOffset.UtcNow;
        _context.TenantMcpServers.Add(server);
        await _context.SaveChangesAsync(cancellationToken);
        return server;
    }

    public async Task AddToolsAsync(
        IEnumerable<TenantMcpTool> tools,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var tool in tools)
        {
            tool.CreatedAt = now;
        }

        _context.TenantMcpTools.AddRange(tools);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantMcpServer>> GetServersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantMcpServers
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantMcpTool>> GetToolsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantMcpTools
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.ToolName)
            .ToListAsync(cancellationToken);

    public Task<TenantMcpTool?> GetToolByNameAsync(
        Guid tenantId,
        string toolName,
        CancellationToken cancellationToken = default) =>
        _context.TenantMcpTools.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.ToolName == toolName,
            cancellationToken);

    public Task<TenantMcpServer?> GetServerByIdAsync(
        Guid tenantId,
        Guid serverId,
        CancellationToken cancellationToken = default) =>
        _context.TenantMcpServers.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.Id == serverId,
            cancellationToken);

    public Task<TenantMcpTool?> GetToolByIdAsync(
        Guid tenantId,
        Guid toolId,
        CancellationToken cancellationToken = default) =>
        _context.TenantMcpTools.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.Id == toolId,
            cancellationToken);
}
