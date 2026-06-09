namespace LLMCoordination.Domain.Entities;

public class TenantSwaggerDocument
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string RawJson { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TenantSwaggerEndpoint> Endpoints { get; set; } = [];
}
