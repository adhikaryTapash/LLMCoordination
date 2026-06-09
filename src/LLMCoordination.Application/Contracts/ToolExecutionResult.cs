namespace LLMCoordination.Application.Contracts;

public class ToolExecutionResult
{
    public string Status { get; set; } = "success";
    public string RawJson { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}
