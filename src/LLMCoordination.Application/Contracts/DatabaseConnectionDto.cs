namespace LLMCoordination.Application.Contracts;

public class DatabaseConnectionDto
{
    public Guid Id { get; set; }
    public string DbType { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public bool ReadOnly { get; set; }
    public string Status { get; set; } = string.Empty;
}
