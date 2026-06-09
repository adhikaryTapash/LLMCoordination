namespace LLMCoordination.Application.Contracts;

public class McpToolCallRequest
{
    public Guid ServerId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}
