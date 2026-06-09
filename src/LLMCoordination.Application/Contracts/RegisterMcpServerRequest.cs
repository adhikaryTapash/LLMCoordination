namespace LLMCoordination.Application.Contracts;

public class RegisterMcpServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string CredentialType { get; set; } = string.Empty;
    public string CredentialValue { get; set; } = string.Empty;
}
