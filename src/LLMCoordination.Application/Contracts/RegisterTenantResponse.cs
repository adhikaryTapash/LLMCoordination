namespace LLMCoordination.Application.Contracts;

public class RegisterTenantResponse
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}
