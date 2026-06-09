using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AgentAuditLog log, CancellationToken cancellationToken = default)
    {
        _context.AgentAuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgentAuditLog>> GetLogsAsync(
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(0, (page - 1) * pageSize);
        return await _context.AgentAuditLogs
            .Where(l => l.TenantId == tenantId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
