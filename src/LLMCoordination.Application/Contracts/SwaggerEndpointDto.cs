namespace LLMCoordination.Application.Contracts;

public class SwaggerEndpointDto
{
    public Guid Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
