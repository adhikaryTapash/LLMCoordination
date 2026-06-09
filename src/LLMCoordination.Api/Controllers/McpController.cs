using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using AppMcpToolCallResult = LLMCoordination.Application.Contracts.McpToolCallResult;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Infrastructure.Mcp;
using LLMCoordination.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/mcp")]
public class McpController : ControllerBase
{
    private readonly IMcpRepository _mcpRepository;
    private readonly McpMetadataService _mcpMetadataService;
    private readonly McpClient _mcpClient;
    private readonly CredentialEncryptionService _encryptionService;
    private readonly TenantContextService _tenantContext;

    public McpController(
        IMcpRepository mcpRepository,
        McpMetadataService mcpMetadataService,
        McpClient mcpClient,
        CredentialEncryptionService encryptionService,
        TenantContextService tenantContext)
    {
        _mcpRepository = mcpRepository;
        _mcpMetadataService = mcpMetadataService;
        _mcpClient = mcpClient;
        _encryptionService = encryptionService;
        _tenantContext = tenantContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<McpServerDto>> Register(
        [FromBody] RegisterMcpServerRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        if (!string.IsNullOrWhiteSpace(request.CredentialValue))
        {
            request.CredentialValue = _encryptionService.Encrypt(request.CredentialValue);
        }

        var server = await _mcpMetadataService.RegisterServerAsync(tenantId.Value, request, cancellationToken);
        return Ok(server);
    }

    [HttpPost("servers/{id:guid}/discover")]
    public async Task<ActionResult<IReadOnlyList<McpToolDto>>> DiscoverTools(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var server = await _mcpRepository.GetServerByIdAsync(tenantId.Value, id, cancellationToken);
        if (server is null)
        {
            return NotFound();
        }

        var discovered = await _mcpClient.ListToolsAsync(server.BaseUrl, cancellationToken);
        var tools = discovered.Select(t => new TenantMcpTool
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            McpServerId = server.Id,
            ToolName = t.Name,
            Description = t.Description,
            InputSchemaJson = t.InputSchemaJson,
            OutputSchemaJson = t.OutputSchemaJson
        }).ToList();

        await _mcpRepository.AddToolsAsync(tools, cancellationToken);

        var result = tools.Select(t => new McpToolDto
        {
            Id = t.Id,
            ToolName = t.ToolName,
            Description = t.Description,
            InputSchemaJson = t.InputSchemaJson
        }).ToList();

        return Ok(result);
    }

    [HttpGet("servers")]
    public async Task<ActionResult<IReadOnlyList<McpServerDto>>> ListServers(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var servers = await _mcpRepository.GetServersAsync(tenantId.Value, cancellationToken);
        var tools = await _mcpRepository.GetToolsAsync(tenantId.Value, cancellationToken);

        var result = servers.Select(s => new McpServerDto
        {
            Id = s.Id,
            Name = s.Name,
            BaseUrl = s.BaseUrl,
            Status = s.Status,
            ToolCount = tools.Count(t => t.McpServerId == s.Id)
        }).ToList();

        return Ok(result);
    }

    [HttpGet("tools")]
    public async Task<ActionResult<IReadOnlyList<McpToolDto>>> ListTools(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var tools = await _mcpMetadataService.GetToolsAsync(tenantId.Value, cancellationToken);
        return Ok(tools);
    }

    [HttpPost("tools/{id:guid}/test")]
    public async Task<ActionResult<AppMcpToolCallResult>> TestTool(
        Guid id,
        [FromBody] McpToolCallRequest request,
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

        var tool = await _mcpRepository.GetToolByIdAsync(tenantId.Value, id, cancellationToken);
        if (tool is null)
        {
            return NotFound();
        }

        var server = await _mcpRepository.GetServerByIdAsync(tenantId.Value, tool.McpServerId, cancellationToken);
        if (server is null)
        {
            return NotFound();
        }

        var arguments = request.Arguments?.ToDictionary(
            kvp => kvp.Key,
            kvp => (object?)kvp.Value) ?? new Dictionary<string, object?>();

        var result = await _mcpClient.CallToolAsync(server.BaseUrl, tool.ToolName, arguments, cancellationToken);

        return Ok(new AppMcpToolCallResult
        {
            Status = result.Status.Equals("Success", StringComparison.OrdinalIgnoreCase) ? "success" : "failed",
            OutputJson = result.OutputJson ?? "{}",
            ErrorMessage = result.ErrorMessage
        });
    }
}
