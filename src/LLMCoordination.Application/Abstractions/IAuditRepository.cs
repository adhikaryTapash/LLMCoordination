using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface IAuditRepository
{
    Task AddAsync(AgentAuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentAuditLog>> GetLogsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
}
