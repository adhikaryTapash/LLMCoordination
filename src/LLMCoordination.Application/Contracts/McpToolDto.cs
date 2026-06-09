namespace LLMCoordination.Application.Contracts;

public class McpToolDto
{
    public Guid Id { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputSchemaJson { get; set; } = string.Empty;
}
