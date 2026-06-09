namespace LLMCoordination.Application.Contracts;

public class ApprovalResponse
{
    public bool Required { get; set; }
    public bool Granted { get; set; }
    public string? Message { get; set; }
    public string? PendingAction { get; set; }
}
