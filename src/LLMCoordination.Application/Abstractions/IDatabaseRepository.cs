using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface IDatabaseRepository
{
    Task<TenantDatabaseConnection> AddConnectionAsync(TenantDatabaseConnection connection, CancellationToken cancellationToken = default);
    Task AddSchemaEntriesAsync(IEnumerable<TenantDatabaseSchema> schemaEntries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDatabaseConnection>> GetConnectionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDatabaseSchema>> GetSchemaAsync(Guid tenantId, Guid? connectionId = null, CancellationToken cancellationToken = default);
    Task<TenantDatabaseConnection?> GetConnectionByIdAsync(Guid tenantId, Guid connectionId, CancellationToken cancellationToken = default);
}
