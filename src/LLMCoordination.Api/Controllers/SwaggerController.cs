using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/swagger")]
public class SwaggerController : ControllerBase
{
    private readonly ISwaggerRepository _swaggerRepository;
    private readonly SwaggerRegistrationService _registrationService;
    private readonly TenantContextService _tenantContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public SwaggerController(
        ISwaggerRepository swaggerRepository,
        SwaggerRegistrationService registrationService,
        TenantContextService tenantContext,
        IHttpClientFactory httpClientFactory)
    {
        _swaggerRepository = swaggerRepository;
        _registrationService = registrationService;
        _tenantContext = tenantContext;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("register")]
    public async Task<ActionResult<SwaggerDocumentDto>> Register(
        [FromBody] RegisterSwaggerRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        string rawJson;
        try
        {
            rawJson = await ResolveSwaggerJsonAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        try
        {
            var document = await _registrationService.RegisterAsync(tenantId.Value, request, rawJson, cancellationToken);
            return Ok(document);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("documents")]
    public async Task<ActionResult<IReadOnlyList<SwaggerDocumentDto>>> ListDocuments(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var documents = await _swaggerRepository.GetDocumentsAsync(tenantId.Value, cancellationToken);
        var endpoints = await _swaggerRepository.GetEndpointsAsync(tenantId.Value, cancellationToken);

        var result = documents.Select(d => new SwaggerDocumentDto
        {
            Id = d.Id,
            Name = d.Name,
            DocumentUrl = d.DocumentUrl,
            Version = d.Version,
            Status = d.Status,
            EndpointCount = endpoints.Count(e => e.SwaggerDocumentId == d.Id)
        }).ToList();

        return Ok(result);
    }

    [HttpGet("documents/{id:guid}/endpoints")]
    public async Task<ActionResult<IReadOnlyList<SwaggerEndpointDto>>> ListEndpoints(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var document = await _swaggerRepository.GetDocumentByIdAsync(tenantId.Value, id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var endpoints = await _swaggerRepository.GetEndpointsAsync(tenantId.Value, cancellationToken);
        var result = endpoints
            .Where(e => e.SwaggerDocumentId == id)
            .Select(e => new SwaggerEndpointDto
            {
                Id = e.Id,
                Method = e.Method,
                Path = e.Path,
                OperationId = e.OperationId,
                Summary = e.Summary
            })
            .ToList();

        return Ok(result);
    }

    [HttpDelete("documents/{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (tenantId is null)
        {
            return Unauthorized();
        }

        var deleted = await _swaggerRepository.DeleteDocumentAsync(tenantId.Value, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private async Task<string> ResolveSwaggerJsonAsync(
        RegisterSwaggerRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RawJson))
        {
            return request.RawJson;
        }

        if (string.IsNullOrWhiteSpace(request.SwaggerUrl))
        {
            throw new InvalidOperationException("Either SwaggerUrl or RawJson must be provided.");
        }

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        return await client.GetStringAsync(request.SwaggerUrl, cancellationToken);
    }
}
