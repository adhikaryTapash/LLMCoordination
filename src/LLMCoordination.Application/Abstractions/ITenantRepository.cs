using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AppUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AppUser> CreateUserAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantPermissionRule>> GetPermissionRulesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
