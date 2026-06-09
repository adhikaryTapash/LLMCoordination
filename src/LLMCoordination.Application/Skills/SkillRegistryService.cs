using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Skills;

public class SkillRegistryService
{
    private readonly ISkillRepository _skillRepository;

    public SkillRegistryService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<IReadOnlyList<SkillDefinition>> GetAllEnabledSkillsAsync(
        CancellationToken cancellationToken = default)
    {
        var skills = await _skillRepository.GetAllEnabledAsync(cancellationToken);
        return skills.Select(MapToDefinition).ToList();
    }

    public async Task<SkillDefinition?> GetSkillByNameAsync(
        string skillName,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillRepository.GetByNameAsync(skillName, cancellationToken);
        return skill is null ? null : MapToDefinition(skill);
    }

    public async Task<SkillDefinition?> FindBestSkillForIntentAsync(
        string intentName,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillRepository.FindByIntentAsync(intentName, cancellationToken);
        if (skill is not null)
        {
            return MapToDefinition(skill);
        }

        var fallbackName = MapIntentToSkill(intentName);
        var fallback = await _skillRepository.GetByNameAsync(fallbackName, cancellationToken);
        return fallback is null ? null : MapToDefinition(fallback);
    }

    private static SkillDefinition MapToDefinition(Domain.Entities.GlobalSkill skill) =>
        new()
        {
            SkillName = skill.SkillName,
            Domain = skill.Domain.ToString(),
            Category = skill.Category.ToString(),
            Description = skill.Description,
            ActionType = skill.ActionType.ToString()
        };

    private static string MapIntentToSkill(string intentName)
    {
        if (intentName.StartsWith("patient.list", StringComparison.OrdinalIgnoreCase) ||
            intentName.StartsWith("appointment.list", StringComparison.OrdinalIgnoreCase))
        {
            return "api.query.execute";
        }

        if (intentName.Contains(".delete", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.delete";
        }

        if (intentName.Contains(".create", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.create";
        }

        if (intentName.Contains(".update", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.update";
        }

        if (intentName.StartsWith("database.", StringComparison.OrdinalIgnoreCase))
        {
            return "database.sql.generate";
        }

        return "api.query.execute";
    }
}
