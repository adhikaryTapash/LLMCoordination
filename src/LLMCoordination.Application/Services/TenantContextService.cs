using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LLMCoordination.Application.Services;

public class TenantContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
    }

    public Guid? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }

    public string? GetCurrentUserRole() =>
        _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value
        ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
}
