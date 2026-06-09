namespace LLMCoordination.Application.Contracts;

public class SqlExecutionResult
{
    public string Status { get; set; } = string.Empty;
    public string RawJson { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public string? ErrorMessage { get; set; }
}
