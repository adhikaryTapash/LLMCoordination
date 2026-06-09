using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly AppDbContext _context;

    public DatabaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDatabaseConnection> AddConnectionAsync(
        TenantDatabaseConnection connection,
        CancellationToken cancellationToken = default)
    {
        connection.CreatedAt = DateTimeOffset.UtcNow;
        _context.TenantDatabaseConnections.Add(connection);
        await _context.SaveChangesAsync(cancellationToken);
        return connection;
    }

    public async Task AddSchemaEntriesAsync(
        IEnumerable<TenantDatabaseSchema> schemaEntries,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in schemaEntries)
        {
            entry.CreatedAt = now;
        }

        _context.TenantDatabaseSchemas.AddRange(schemaEntries);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantDatabaseConnection>> GetConnectionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantDatabaseConnections
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantDatabaseSchema>> GetSchemaAsync(
        Guid tenantId,
        Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TenantDatabaseSchemas.Where(s => s.TenantId == tenantId);

        if (connectionId.HasValue)
        {
            query = query.Where(s => s.ConnectionId == connectionId.Value);
        }

        return await query.OrderBy(s => s.TableName).ThenBy(s => s.ColumnName).ToListAsync(cancellationToken);
    }

    public Task<TenantDatabaseConnection?> GetConnectionByIdAsync(
        Guid tenantId,
        Guid connectionId,
        CancellationToken cancellationToken = default) =>
        _context.TenantDatabaseConnections.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Id == connectionId,
            cancellationToken);
}
