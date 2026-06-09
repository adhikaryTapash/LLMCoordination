namespace LLMCoordination.Application.Contracts;

public class EndpointMappingDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public Guid EndpointId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public bool Enabled { get; set; }
}
