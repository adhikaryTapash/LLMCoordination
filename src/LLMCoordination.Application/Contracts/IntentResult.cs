namespace LLMCoordination.Application.Contracts;

public class IntentResult
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = "read";
}
