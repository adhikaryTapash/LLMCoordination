using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Contracts;

public class RegisterTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public DomainType DomainType { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
}
