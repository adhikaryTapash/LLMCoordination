using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class SwaggerRepository : ISwaggerRepository
{
    private readonly AppDbContext _context;

    public SwaggerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TenantSwaggerDocument> AddDocumentAsync(
        TenantSwaggerDocument document,
        CancellationToken cancellationToken = default)
    {
        document.CreatedAt = DateTimeOffset.UtcNow;
        _context.TenantSwaggerDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task AddEndpointsAsync(
        IEnumerable<TenantSwaggerEndpoint> endpoints,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var endpoint in endpoints)
        {
            endpoint.CreatedAt = now;
        }

        _context.TenantSwaggerEndpoints.AddRange(endpoints);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantSwaggerDocument>> GetDocumentsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantSwaggerDocuments
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantSwaggerEndpoint>> GetEndpointsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantSwaggerEndpoints
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.Path)
            .ToListAsync(cancellationToken);

    public Task<TenantSwaggerEndpoint?> GetEndpointByIdAsync(
        Guid endpointId,
        CancellationToken cancellationToken = default) =>
        _context.TenantSwaggerEndpoints.FirstOrDefaultAsync(e => e.Id == endpointId, cancellationToken);

    public Task<TenantSwaggerDocument?> GetDocumentByIdAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken = default) =>
        _context.TenantSwaggerDocuments.FirstOrDefaultAsync(
            d => d.TenantId == tenantId && d.Id == documentId,
            cancellationToken);

    public async Task<bool> DeleteDocumentAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _context.TenantSwaggerDocuments
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == documentId, cancellationToken);

        if (document is null)
        {
            return false;
        }

        var endpoints = await _context.TenantSwaggerEndpoints
            .Where(e => e.SwaggerDocumentId == documentId)
            .ToListAsync(cancellationToken);

        var mappings = await _context.TenantSkillEndpointMappings
            .Where(m => endpoints.Select(e => e.Id).Contains(m.EndpointId))
            .ToListAsync(cancellationToken);

        _context.TenantSkillEndpointMappings.RemoveRange(mappings);
        _context.TenantSwaggerEndpoints.RemoveRange(endpoints);
        _context.TenantSwaggerDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
