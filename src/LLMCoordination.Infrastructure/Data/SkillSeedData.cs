using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Infrastructure.Data;

public sealed record SkillSeedDefinition(
    string SkillName,
    SkillCategory Category,
    ActionType ActionType,
    string Description,
    DomainType Domain = DomainType.Mixed,
    IReadOnlyList<string>? IntentExamples = null);

public static class SkillSeedData
{
    public static IReadOnlyList<SkillSeedDefinition> Skills { get; } =
    [
        new(
            "swagger.endpoint.search",
            SkillCategory.Api,
            ActionType.Read,
            "Search API endpoints from tenant Swagger documents",
            IntentExamples: ["Find endpoints related to patients", "Search for appointment APIs", "Which endpoints handle billing?"]),
        new(
            "swagger.endpoint.explain",
            SkillCategory.Api,
            ActionType.Read,
            "Explain endpoint purpose, parameters, and response",
            IntentExamples: ["Explain the patient list endpoint", "What parameters does this API need?", "Describe the response format"]),
        new(
            "api.query.execute",
            SkillCategory.Api,
            ActionType.Read,
            "Execute GET/list/search API endpoints",
            IntentExamples: ["Give me the list of patients", "Show all appointments today", "Search customers by name"]),
        new(
            "api.record.create",
            SkillCategory.Api,
            ActionType.Create,
            "Execute POST/create API endpoints",
            IntentExamples: ["Create a new patient record", "Add a new appointment", "Register a new customer"]),
        new(
            "api.record.update",
            SkillCategory.Api,
            ActionType.Update,
            "Execute PATCH/PUT update API endpoints",
            IntentExamples: ["Update patient contact details", "Change appointment time", "Modify order status"]),
        new(
            "api.record.delete",
            SkillCategory.Api,
            ActionType.Delete,
            "Execute DELETE API endpoints with approval",
            IntentExamples: ["Delete this patient record", "Remove the appointment", "Cancel this order"]),
        new(
            "database.schema.search",
            SkillCategory.Database,
            ActionType.Read,
            "Search database tables, columns, and relations",
            IntentExamples: ["What tables store patient data?", "Show columns in appointments table", "Find foreign keys for orders"]),
        new(
            "database.sql.generate",
            SkillCategory.Database,
            ActionType.Read,
            "Generate safe SQL from business question",
            IntentExamples: ["Generate SQL for patient count by department", "Write query for overdue invoices", "SQL to find low stock items"]),
        new(
            "mcp.tool.discovery",
            SkillCategory.Mcp,
            ActionType.Read,
            "Discover available MCP tools",
            IntentExamples: ["What MCP tools are available?", "List tools for scheduling", "Show MCP integrations"]),
        new(
            "mcp.tool.execute",
            SkillCategory.Mcp,
            ActionType.Execute,
            "Execute approved MCP tools",
            IntentExamples: ["Run the patient summary tool", "Execute schedule appointment", "Call the inventory lookup tool"]),
        new(
            "analytics.trend.analysis",
            SkillCategory.Analytics,
            ActionType.Read,
            "Analyze business trends",
            IntentExamples: ["Show patient visit trends this month", "Analyze sales growth", "Trend report for lab tests"]),
        new(
            "response.card.generate",
            SkillCategory.Response,
            ActionType.Read,
            "Format result as card view",
            IntentExamples: ["Show results as cards", "Display patient list in card view", "Format as card layout"])
    ];

    public const string DefaultPromptTemplate =
        "You are executing skill {{skillName}}. Use the provided context and tenant tools to fulfill the user request safely.";

    public const string DefaultResponseSchemaJson =
        """{"type":"object","properties":{"status":{"type":"string"},"answer":{"type":"string"},"data":{"type":"object"}}}""";
}
