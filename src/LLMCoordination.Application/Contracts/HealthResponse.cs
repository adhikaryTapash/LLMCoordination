namespace LLMCoordination.Application.Contracts;

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public bool DatabaseConnected { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
