namespace LLMCoordination.Application.Contracts;

public class EndpointInfo
{
    public Guid Id { get; set; }
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}
