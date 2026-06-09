namespace LLMCoordination.Application.Contracts;

public class RegisterSwaggerRequest
{
    public string Name { get; set; } = string.Empty;
    public string SwaggerUrl { get; set; } = string.Empty;
    public string? RawJson { get; set; }
    public string Domain { get; set; } = string.Empty;
}
