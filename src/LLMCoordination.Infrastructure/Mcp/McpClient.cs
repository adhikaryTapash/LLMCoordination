using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LLMCoordination.Infrastructure.Mcp;

public sealed class McpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<McpClient> _logger;

    public McpClient(IHttpClientFactory httpClientFactory, ILogger<McpClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        if (IsMockUrl(baseUrl))
        {
            _logger.LogDebug("Using mock MCP tool catalog for {BaseUrl}", baseUrl);
            return Task.FromResult(MockMcpToolCatalog.GetTools());
        }

        return ListToolsFromServerAsync(baseUrl, cancellationToken);
    }

    public Task<McpToolCallResult> CallToolAsync(
        string baseUrl,
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        if (IsMockUrl(baseUrl))
        {
            _logger.LogDebug("Executing mock MCP tool {ToolName}", toolName);
            return Task.FromResult(CreateMockToolResult(toolName, arguments));
        }

        return CallToolOnServerAsync(baseUrl, toolName, arguments, cancellationToken);
    }

    private async Task<IReadOnlyList<McpToolDefinition>> ListToolsFromServerAsync(
        string baseUrl,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(McpClient));
        client.BaseAddress = new Uri(NormalizeBaseUrl(baseUrl));

        var request = new { jsonrpc = "2.0", id = Guid.NewGuid().ToString(), method = "tools/list", @params = new { } };
        using var response = await client.PostAsJsonAsync(string.Empty, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<McpToolsListResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("MCP tools/list returned an empty response.");

        return payload.Result?.Tools?
            .Select(t => new McpToolDefinition(
                t.Name ?? string.Empty,
                t.Description ?? string.Empty,
                t.InputSchema?.RootElement.GetRawText() ?? "{}",
                t.OutputSchema?.RootElement.GetRawText() ?? "{}"))
            .ToList()
            ?? [];
    }

    private async Task<McpToolCallResult> CallToolOnServerAsync(
        string baseUrl,
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(McpClient));
        client.BaseAddress = new Uri(NormalizeBaseUrl(baseUrl));

        var request = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new { name = toolName, arguments }
        };

        using var response = await client.PostAsJsonAsync(string.Empty, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<McpToolCallResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("MCP tools/call returned an empty response.");

        if (payload.Error is not null)
        {
            return new McpToolCallResult("Failed", null, payload.Error.Message);
        }

        var outputJson = payload.Result?.Content is null
            ? "{}"
            : JsonSerializer.Serialize(payload.Result.Content, JsonOptions);

        return new McpToolCallResult("Success", outputJson, null);
    }

    private static McpToolCallResult CreateMockToolResult(string toolName, Dictionary<string, object?> arguments)
    {
        var knownTool = MockMcpToolCatalog.GetTools()
            .FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));

        if (knownTool is null)
        {
            return new McpToolCallResult("Failed", null, $"Mock MCP tool '{toolName}' was not found.");
        }

        object output = toolName.ToLowerInvariant() switch
        {
            "get_patient_summary" => new
            {
                patientId = arguments.GetValueOrDefault("patientId")?.ToString() ?? "unknown",
                summary = "Mock patient summary generated successfully."
            },
            "schedule_appointment" => new
            {
                appointmentId = Guid.NewGuid().ToString(),
                status = "scheduled"
            },
            "lookup_inventory" => new
            {
                sku = arguments.GetValueOrDefault("sku")?.ToString() ?? "unknown",
                quantity = 42,
                location = "Main Warehouse"
            },
            _ => new { status = "ok", tool = toolName }
        };

        return new McpToolCallResult("Success", JsonSerializer.Serialize(output, JsonOptions), null);
    }

    private static bool IsMockUrl(string baseUrl) =>
        baseUrl.Contains("mock", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeBaseUrl(string baseUrl) =>
        baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

    private sealed class McpToolsListResponse
    {
        public McpToolsListResult? Result { get; set; }
    }

    private sealed class McpToolsListResult
    {
        public List<McpToolMetadata>? Tools { get; set; }
    }

    private sealed class McpToolMetadata
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public JsonDocument? InputSchema { get; set; }
        public JsonDocument? OutputSchema { get; set; }
    }

    private sealed class McpToolCallResponse
    {
        public McpToolCallResultPayload? Result { get; set; }
        public McpErrorPayload? Error { get; set; }
    }

    private sealed class McpToolCallResultPayload
    {
        public object? Content { get; set; }
    }

    private sealed class McpErrorPayload
    {
        public string? Message { get; set; }
    }
}

public sealed record McpToolCallResult(string Status, string? OutputJson, string? ErrorMessage);
