using LLMCoordination.Application.Abstractions;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly AppDbContext _context;

    public SkillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GlobalSkill>> GetAllEnabledAsync(CancellationToken cancellationToken = default) =>
        await _context.GlobalSkills
            .Include(s => s.IntentExamples)
            .Where(s => s.Enabled)
            .OrderBy(s => s.SkillName)
            .ToListAsync(cancellationToken);

    public Task<GlobalSkill?> GetByNameAsync(string skillName, CancellationToken cancellationToken = default) =>
        _context.GlobalSkills
            .Include(s => s.IntentExamples)
            .FirstOrDefaultAsync(s => s.SkillName == skillName, cancellationToken);

    public async Task<GlobalSkill?> FindByIntentAsync(string intentName, CancellationToken cancellationToken = default)
    {
        var normalizedIntent = intentName.ToLowerInvariant();

        var skill = await _context.GlobalSkills
            .Include(s => s.IntentExamples)
            .Where(s => s.Enabled)
            .Where(s => s.IntentExamples.Any(e =>
                normalizedIntent.Contains(e.ExampleText.ToLowerInvariant()) ||
                e.ExampleText.ToLowerInvariant().Contains(normalizedIntent)))
            .FirstOrDefaultAsync(cancellationToken);

        if (skill is not null)
        {
            return skill;
        }

        return await _context.GlobalSkills
            .Include(s => s.IntentExamples)
            .Where(s => s.Enabled && s.SkillName.Contains(normalizedIntent.Replace('.', ' ')))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddEndpointMappingAsync(
        TenantSkillEndpointMapping mapping,
        CancellationToken cancellationToken = default)
    {
        _context.TenantSkillEndpointMappings.Add(mapping);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantSkillEndpointMapping>> GetEndpointMappingsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantSkillEndpointMappings
            .Include(m => m.Skill)
            .Include(m => m.Endpoint)
            .Where(m => m.TenantId == tenantId)
            .ToListAsync(cancellationToken);
}
