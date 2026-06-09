namespace LLMCoordination.Application.Contracts;

public class RegisterDatabaseRequest
{
    public string DbType { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool ReadOnly { get; set; } = true;
}
