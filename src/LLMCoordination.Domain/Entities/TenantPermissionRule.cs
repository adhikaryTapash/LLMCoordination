namespace LLMCoordination.Domain.Entities;

public class TenantPermissionRule
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Allowed { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
