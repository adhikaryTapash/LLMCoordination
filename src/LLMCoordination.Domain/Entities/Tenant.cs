using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DomainType DomainType { get; set; }
    public string Status { get; set; } = "Active";
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<TenantSwaggerDocument> SwaggerDocuments { get; set; } = [];
    public ICollection<TenantMcpServer> McpServers { get; set; } = [];
    public ICollection<TenantDatabaseConnection> DatabaseConnections { get; set; } = [];
    public ICollection<TenantPermissionRule> PermissionRules { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
}
