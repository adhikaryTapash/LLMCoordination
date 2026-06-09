namespace LLMCoordination.Infrastructure.Swagger;

public class SwaggerParseException : Exception
{
    public SwaggerParseException(string message)
        : base(message)
    {
    }

    public SwaggerParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
