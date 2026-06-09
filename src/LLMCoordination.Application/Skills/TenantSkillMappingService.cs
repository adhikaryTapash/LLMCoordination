using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Skills;

public class TenantSkillMappingService
{
    private readonly ISkillRepository _skillRepository;

    public TenantSkillMappingService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task MapSwaggerEndpointsAsync(
        Guid tenantId,
        IEnumerable<TenantSwaggerEndpoint> endpoints,
        CancellationToken cancellationToken = default)
    {
        foreach (var endpoint in endpoints)
        {
            var skillName = InferSkillFromMethod(endpoint.Method);
            var skill = await _skillRepository.GetByNameAsync(skillName, cancellationToken);
            if (skill is null)
            {
                continue;
            }

            await _skillRepository.AddEndpointMappingAsync(new TenantSkillEndpointMapping
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SkillId = skill.Id,
                EndpointId = endpoint.Id,
                ConfidenceScore = 0.8,
                Enabled = true
            }, cancellationToken);
        }
    }

    private static string InferSkillFromMethod(string method) =>
        method.ToUpperInvariant() switch
        {
            "GET" or "HEAD" => "api.query.execute",
            "POST" => "api.record.create",
            "PUT" or "PATCH" => "api.record.update",
            "DELETE" => "api.record.delete",
            _ => "api.query.execute"
        };
}
