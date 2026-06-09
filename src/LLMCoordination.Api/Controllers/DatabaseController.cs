using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Agents;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using LLMCoordination.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly DbSchemaReaderService _schemaReaderService;
    private readonly IDatabaseRepository _databaseRepository;
    private readonly SqlValidationAgent _sqlValidationAgent;
    private readonly CredentialEncryptionService _encryptionService;
    private readonly TenantContextService _tenantContext;

    public DatabaseController(
        DbSchemaReaderService schemaReaderService,
        IDatabaseRepository databaseRepository,
        SqlValidationAgent sqlValidationAgent,
        CredentialEncryptionService encryptionService,
        TenantContextService tenantContext)
    {
        _schemaReaderService = schemaReaderService;
        _databaseRepository = databaseRepository;
        _sqlValidationAgent = sqlValidationAgent;
        _encryptionService = encryptionService;
        _tenantContext = tenantContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<DatabaseConnectionDto>> Register(
        [FromBody] RegisterDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        request.ReadOnly = true;
        if (!string.IsNullOrWhiteSpace(request.ConnectionString))
        {
            request.ConnectionString = _encryptionService.Encrypt(request.ConnectionString);
        }

        var connection = await _schemaReaderService.RegisterConnectionAsync(tenantId.Value, request, cancellationToken);
        return Ok(connection);
    }

    [HttpPost("connections/{id:guid}/introspect")]
    public async Task<ActionResult<IReadOnlyList<SchemaTableDto>>> Introspect(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var connection = await _databaseRepository.GetConnectionByIdAsync(tenantId.Value, id, cancellationToken);
        if (connection is null)
        {
            return NotFound();
        }

        var schema = await _schemaReaderService.GetSchemaTablesAsync(tenantId.Value, id, cancellationToken);
        return Ok(schema);
    }

    [HttpGet("connections")]
    public async Task<ActionResult<IReadOnlyList<DatabaseConnectionDto>>> ListConnections(
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var connections = await _databaseRepository.GetConnectionsAsync(tenantId.Value, cancellationToken);
        var result = connections.Select(c => new DatabaseConnectionDto
        {
            Id = c.Id,
            DbType = c.DbType,
            Host = c.Host,
            DatabaseName = c.DatabaseName,
            ReadOnly = c.ReadOnly,
            Status = c.Status
        }).ToList();

        return Ok(result);
    }

    [HttpGet("schema")]
    public async Task<ActionResult<IReadOnlyList<SchemaTableDto>>> ListSchema(
        [FromQuery] Guid? connectionId,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var schema = await _schemaReaderService.GetSchemaTablesAsync(tenantId.Value, connectionId, cancellationToken);
        return Ok(schema);
    }

    [HttpPost("test-query")]
    public async Task<ActionResult<object>> TestQuery(
        [FromBody] TestQueryRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var role = _tenantContext.GetCurrentUserRole();

        if (tenantId is null)
        {
            return Unauthorized();
        }

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return BadRequest(new { message = "SQL is required." });
        }

        var context = new AgentContext
        {
            TenantId = tenantId.Value,
            UserId = _tenantContext.GetCurrentUserId() ?? Guid.Empty,
            UserRole = role ?? "Admin",
            ConversationId = Guid.NewGuid()
        };

        context.AgentOutputs[SqlGenerationAgent.AgentName] = new SqlGenerationResult
        {
            Sql = request.Sql,
            Explanation = "Admin test query",
            IsReadOnly = true
        };

        var validation = await _sqlValidationAgent.ExecuteAsync(context, cancellationToken);
        if (validation.Status == Domain.Enums.AgentStatus.Failed)
        {
            return BadRequest(new { message = validation.ErrorMessage });
        }

        return Ok(new
        {
            status = "validated",
            sql = request.Sql,
            message = "SQL passed validation checks."
        });
    }
}
