namespace LLMCoordination.Infrastructure.Mcp;

public sealed record McpToolDefinition(
    string Name,
    string Description,
    string InputSchemaJson,
    string OutputSchemaJson);

public static class MockMcpToolCatalog
{
    public static IReadOnlyList<McpToolDefinition> GetTools() =>
    [
        new(
            "get_patient_summary",
            "Retrieve a summary for a patient by identifier.",
            """{"type":"object","properties":{"patientId":{"type":"string"}},"required":["patientId"]}""",
            """{"type":"object","properties":{"patientId":{"type":"string"},"summary":{"type":"string"}}}"""),
        new(
            "schedule_appointment",
            "Schedule an appointment for a patient with a provider.",
            """{"type":"object","properties":{"patientId":{"type":"string"},"providerId":{"type":"string"},"dateTime":{"type":"string","format":"date-time"}},"required":["patientId","providerId","dateTime"]}""",
            """{"type":"object","properties":{"appointmentId":{"type":"string"},"status":{"type":"string"}}}"""),
        new(
            "lookup_inventory",
            "Look up inventory levels for a product or SKU.",
            """{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"]}""",
            """{"type":"object","properties":{"sku":{"type":"string"},"quantity":{"type":"integer"},"location":{"type":"string"}}}""")
    ];
}
