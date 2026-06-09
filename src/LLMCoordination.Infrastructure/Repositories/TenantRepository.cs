using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<Tenant?> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        tenant.CreatedAt = DateTimeOffset.UtcNow;
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<AppUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<AppUser> CreateUserAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        user.CreatedAt = DateTimeOffset.UtcNow;
        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<IReadOnlyList<TenantPermissionRule>> GetPermissionRulesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantPermissionRules
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(cancellationToken);
}
