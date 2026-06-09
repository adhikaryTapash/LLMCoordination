namespace LLMCoordination.Application.Contracts;

public class CreateEndpointMappingRequest
{
    public string SkillName { get; set; } = string.Empty;
    public Guid EndpointId { get; set; }
    public double ConfidenceScore { get; set; } = 1.0;
}
