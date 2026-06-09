using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface ISwaggerRepository
{
    Task<TenantSwaggerDocument> AddDocumentAsync(TenantSwaggerDocument document, CancellationToken cancellationToken = default);
    Task AddEndpointsAsync(IEnumerable<TenantSwaggerEndpoint> endpoints, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSwaggerDocument>> GetDocumentsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSwaggerEndpoint>> GetEndpointsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantSwaggerEndpoint?> GetEndpointByIdAsync(Guid endpointId, CancellationToken cancellationToken = default);
    Task<TenantSwaggerDocument?> GetDocumentByIdAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default);
}
