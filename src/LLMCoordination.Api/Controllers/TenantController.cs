using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class TenantController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantContextService _tenantContext;

    public TenantController(ITenantRepository tenantRepository, TenantContextService tenantContext)
    {
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    [HttpGet("me")]
    public async Task<ActionResult<TenantInfoDto>> GetCurrentTenant(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value, cancellationToken);
        if (tenant is null)
        {
            return NotFound();
        }

        return Ok(new TenantInfoDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            DomainType = tenant.DomainType.ToString(),
            Status = tenant.Status,
            CreatedAt = tenant.CreatedAt
        });
    }
}
