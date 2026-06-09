using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Abstractions;

public interface ISkillRepository
{
    Task<IReadOnlyList<GlobalSkill>> GetAllEnabledAsync(CancellationToken cancellationToken = default);
    Task<GlobalSkill?> GetByNameAsync(string skillName, CancellationToken cancellationToken = default);
    Task<GlobalSkill?> FindByIntentAsync(string intentName, CancellationToken cancellationToken = default);
    Task AddEndpointMappingAsync(TenantSkillEndpointMapping mapping, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSkillEndpointMapping>> GetEndpointMappingsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
