namespace LLMCoordination.Application.Contracts;

public class EndpointDiscoveryResult
{
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
