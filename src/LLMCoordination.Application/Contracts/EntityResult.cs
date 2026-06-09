namespace LLMCoordination.Application.Contracts;

public class EntityResult
{
    public string Resource { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? TimeScope { get; set; }
    public Dictionary<string, object>? Patient { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public Dictionary<string, string>? Sort { get; set; }
    public int? Limit { get; set; }
    public string ViewType { get; set; } = "list";
}
