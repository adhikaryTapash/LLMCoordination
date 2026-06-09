using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using LLMCoordination.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LLMCoordination.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register-tenant")]
    [EnableRateLimiting("register")]
    public async Task<ActionResult<RegisterTenantResponse>> RegisterTenant(
        [FromBody] RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantName) ||
            string.IsNullOrWhiteSpace(request.AdminEmail) ||
            string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            return BadRequest(new { message = "TenantName, AdminEmail, and AdminPassword are required." });
        }

        var response = await _authService.RegisterTenantAsync(request, cancellationToken);

        var tenantId = response.TenantId;
        await using var scope = HttpContext.RequestServices.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.TenantPermissionRules.AddRange(DbInitializer.CreateDefaultPermissionRules(tenantId));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(response);
    }
}
