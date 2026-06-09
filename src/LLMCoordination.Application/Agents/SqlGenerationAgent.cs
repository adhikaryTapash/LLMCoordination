using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class SqlGenerationAgent : IAgent
{
    public const string AgentName = "sql.generation";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var tableName = nlu?.Intent.Resource switch
        {
            "patient" or "patients" => "patients",
            "appointment" or "appointments" => "appointments",
            _ => "patients"
        };

        var operation = nlu?.Entities.Operation ?? "list";
        var sql = operation.Equals("count", StringComparison.OrdinalIgnoreCase)
            ? $"SELECT COUNT(*) FROM {tableName} WHERE tenant_id = '{context.TenantId}'"
            : $"SELECT * FROM {tableName} WHERE tenant_id = '{context.TenantId}' LIMIT 20";

        var result = new SqlGenerationResult
        {
            Sql = sql,
            Explanation = $"Generated read-only SQL for {operation} on {tableName}",
            IsReadOnly = true
        };

        context.AgentOutputs[AgentName] = result;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(result),
            NextAgentHint = "sql.validation"
        });
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
