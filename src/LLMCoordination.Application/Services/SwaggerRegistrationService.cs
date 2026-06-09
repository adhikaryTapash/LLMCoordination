using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Skills;
using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Services;

public class SwaggerRegistrationService
{
    private readonly ISwaggerRepository _swaggerRepository;
    private readonly SwaggerParserService _parserService;
    private readonly TenantSkillMappingService _mappingService;

    public SwaggerRegistrationService(
        ISwaggerRepository swaggerRepository,
        SwaggerParserService parserService,
        TenantSkillMappingService mappingService)
    {
        _swaggerRepository = swaggerRepository;
        _parserService = parserService;
        _mappingService = mappingService;
    }

    public async Task<SwaggerDocumentDto> RegisterAsync(
        Guid tenantId,
        RegisterSwaggerRequest request,
        string rawJson,
        CancellationToken cancellationToken = default)
    {
        var parsed = _parserService.Parse(rawJson);

        var document = new TenantSwaggerDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = string.IsNullOrWhiteSpace(request.Name) ? parsed.Title : request.Name,
            DocumentUrl = request.SwaggerUrl,
            RawJson = rawJson,
            Version = parsed.Version,
            Status = "Active"
        };

        await _swaggerRepository.AddDocumentAsync(document, cancellationToken);

        var endpoints = parsed.Endpoints.Select(e => new TenantSwaggerEndpoint
        {
            Id = e.Id == Guid.Empty ? Guid.NewGuid() : e.Id,
            TenantId = tenantId,
            SwaggerDocumentId = document.Id,
            Method = e.Method,
            Path = e.Path,
            OperationId = e.OperationId,
            Summary = e.Summary,
            Tags = request.Domain
        }).ToList();

        await _swaggerRepository.AddEndpointsAsync(endpoints, cancellationToken);
        await _mappingService.MapSwaggerEndpointsAsync(tenantId, endpoints, cancellationToken);

        return new SwaggerDocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            DocumentUrl = document.DocumentUrl,
            Version = document.Version,
            Status = document.Status,
            EndpointCount = endpoints.Count
        };
    }
}
