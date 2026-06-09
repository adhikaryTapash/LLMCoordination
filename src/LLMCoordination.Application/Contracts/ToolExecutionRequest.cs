namespace LLMCoordination.Application.Contracts;

public class ToolExecutionRequest
{
    public string ToolType { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}
