namespace LLMCoordination.Application.Contracts;

public class SqlGenerationResult
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; } = true;
}
