using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using LLMCoordination.Application.Skills;
using LLMCoordination.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/skills")]
public class SkillController : ControllerBase
{
    private readonly SkillRegistryService _skillRegistry;
    private readonly ISkillRepository _skillRepository;
    private readonly ISwaggerRepository _swaggerRepository;
    private readonly TenantContextService _tenantContext;

    public SkillController(
        SkillRegistryService skillRegistry,
        ISkillRepository skillRepository,
        ISwaggerRepository swaggerRepository,
        TenantContextService tenantContext)
    {
        _skillRegistry = skillRegistry;
        _skillRepository = skillRepository;
        _swaggerRepository = swaggerRepository;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SkillDefinition>>> ListSkills(CancellationToken cancellationToken)
    {
        var skills = await _skillRegistry.GetAllEnabledSkillsAsync(cancellationToken);
        return Ok(skills);
    }

    [HttpGet("{skillName}")]
    public async Task<ActionResult<SkillDetailDto>> GetSkill(string skillName, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByNameAsync(skillName, cancellationToken);
        if (skill is null)
        {
            return NotFound();
        }

        return Ok(new SkillDetailDto
        {
            SkillName = skill.SkillName,
            Domain = skill.Domain.ToString(),
            Category = skill.Category.ToString(),
            Description = skill.Description,
            ActionType = skill.ActionType.ToString(),
            IntentExamples = skill.IntentExamples.Select(e => e.ExampleText).ToList()
        });
    }

    [HttpGet("mappings/endpoints")]
    public async Task<ActionResult<IReadOnlyList<EndpointMappingDto>>> ListEndpointMappings(
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var mappings = await _skillRepository.GetEndpointMappingsAsync(tenantId.Value, cancellationToken);
        var result = mappings.Select(m => new EndpointMappingDto
        {
            Id = m.Id,
            SkillName = m.Skill.SkillName,
            EndpointId = m.EndpointId,
            Method = m.Endpoint.Method,
            Path = m.Endpoint.Path,
            ConfidenceScore = m.ConfidenceScore,
            Enabled = m.Enabled
        }).ToList();

        return Ok(result);
    }

    [HttpPost("mappings/endpoints")]
    public async Task<ActionResult<EndpointMappingDto>> CreateEndpointMapping(
        [FromBody] CreateEndpointMappingRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var skill = await _skillRepository.GetByNameAsync(request.SkillName, cancellationToken);
        if (skill is null)
        {
            return BadRequest(new { message = $"Skill '{request.SkillName}' was not found." });
        }

        var endpoint = await _swaggerRepository.GetEndpointByIdAsync(request.EndpointId, cancellationToken);
        if (endpoint is null || endpoint.TenantId != tenantId.Value)
        {
            return BadRequest(new { message = "Endpoint was not found for this tenant." });
        }

        var mapping = new TenantSkillEndpointMapping
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            SkillId = skill.Id,
            EndpointId = request.EndpointId,
            ConfidenceScore = request.ConfidenceScore,
            Enabled = true
        };

        await _skillRepository.AddEndpointMappingAsync(mapping, cancellationToken);

        return Ok(new EndpointMappingDto
        {
            Id = mapping.Id,
            SkillName = skill.SkillName,
            EndpointId = mapping.EndpointId,
            Method = endpoint.Method,
            Path = endpoint.Path,
            ConfidenceScore = mapping.ConfidenceScore,
            Enabled = mapping.Enabled
        });
    }
}
