namespace LLMCoordination.Application.Contracts;

public class McpToolCallResult
{
    public string Status { get; set; } = string.Empty;
    public string OutputJson { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
