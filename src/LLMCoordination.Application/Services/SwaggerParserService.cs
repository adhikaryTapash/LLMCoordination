using System.Text.Json;
using LLMCoordination.Application.Contracts;

namespace LLMCoordination.Application.Services;

public class SwaggerParserService
{
    public SwaggerParseResult Parse(string rawJson)
    {
        var endpoints = new List<SwaggerEndpointDto>();
        string version = "unknown";
        string title = "Unknown API";

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("info", out var info))
        {
            if (info.TryGetProperty("title", out var titleProp))
            {
                title = titleProp.GetString() ?? title;
            }

            if (info.TryGetProperty("version", out var versionProp))
            {
                version = versionProp.GetString() ?? version;
            }
        }

        if (!root.TryGetProperty("paths", out var paths))
        {
            return new SwaggerParseResult { Title = title, Version = version, Endpoints = endpoints };
        }

        foreach (var pathProp in paths.EnumerateObject())
        {
            foreach (var methodProp in pathProp.Value.EnumerateObject())
            {
                if (!IsHttpMethod(methodProp.Name))
                {
                    continue;
                }

                var operation = methodProp.Value;
                endpoints.Add(new SwaggerEndpointDto
                {
                    Id = Guid.NewGuid(),
                    Method = methodProp.Name.ToUpperInvariant(),
                    Path = pathProp.Name,
                    OperationId = operation.TryGetProperty("operationId", out var opId)
                        ? opId.GetString() ?? string.Empty
                        : string.Empty,
                    Summary = operation.TryGetProperty("summary", out var summary)
                        ? summary.GetString() ?? string.Empty
                        : string.Empty
                });
            }
        }

        return new SwaggerParseResult
        {
            Title = title,
            Version = version,
            Endpoints = endpoints
        };
    }

    private static bool IsHttpMethod(string method) =>
        method is "get" or "post" or "put" or "patch" or "delete" or "head" or "options";
}

public class SwaggerParseResult
{
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<SwaggerEndpointDto> Endpoints { get; set; } = [];
}
