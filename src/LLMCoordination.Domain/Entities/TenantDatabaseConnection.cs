namespace LLMCoordination.Domain.Entities;

public class TenantDatabaseConnection
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string DbType { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string EncryptedConnectionString { get; set; } = string.Empty;
    public bool ReadOnly { get; set; }
    public string Status { get; set; } = "Active";
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TenantDatabaseSchema> SchemaEntries { get; set; } = [];
}
