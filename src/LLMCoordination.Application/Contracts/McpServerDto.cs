namespace LLMCoordination.Application.Contracts;

public class McpServerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ToolCount { get; set; }
}
