using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;
    private readonly TenantContextService _tenantContext;

    public AuditController(IAuditRepository auditRepository, TenantContextService tenantContext)
    {
        _auditRepository = auditRepository;
        _tenantContext = tenantContext;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var logs = await _auditRepository.GetLogsAsync(tenantId.Value, page, pageSize, cancellationToken);
        var result = logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            UserId = l.UserId,
            ConversationId = l.ConversationId,
            AgentName = l.AgentName,
            SkillName = l.SkillName,
            ToolName = l.ToolName,
            Status = l.Status.ToString(),
            ExecutionMs = l.ExecutionMs,
            CreatedAt = l.CreatedAt
        }).ToList();

        return Ok(result);
    }
}
