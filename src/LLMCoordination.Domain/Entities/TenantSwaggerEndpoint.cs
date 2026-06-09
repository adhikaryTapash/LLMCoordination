namespace LLMCoordination.Domain.Entities;

public class TenantSwaggerEndpoint
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SwaggerDocumentId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestSchemaJson { get; set; } = string.Empty;
    public string ResponseSchemaJson { get; set; } = string.Empty;
    public bool AuthRequired { get; set; }
    public string Tags { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TenantSwaggerDocument SwaggerDocument { get; set; } = null!;
    public ICollection<TenantSkillEndpointMapping> SkillMappings { get; set; } = [];
}
