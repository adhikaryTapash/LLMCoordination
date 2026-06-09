namespace LLMCoordination.Application.Contracts;

public class SwaggerDocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EndpointCount { get; set; }
}
