using LLMCoordination.Application.Contracts;
using LLMCoordination.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var dbConnected = false;
        try
        {
            dbConnected = await _dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            dbConnected = false;
        }

        return Ok(new HealthResponse
        {
            Status = dbConnected ? "healthy" : "degraded",
            DatabaseConnected = dbConnected,
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
