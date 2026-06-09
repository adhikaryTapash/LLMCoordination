using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class SqlValidationAgent : IAgent
{
    public const string AgentName = "sql.validation";

    private static readonly string[] BlockedKeywords = ["DROP", "DELETE", "TRUNCATE", "ALTER", "INSERT", "UPDATE"];

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        if (!context.AgentOutputs.TryGetValue(SqlGenerationAgent.AgentName, out var output) ||
            output is not SqlGenerationResult sqlResult)
        {
            return Task.FromResult(new AgentResult
            {
                AgentName = AgentName,
                Status = AgentStatus.Failed,
                ErrorMessage = "No SQL to validate."
            });
        }

        var upperSql = sqlResult.Sql.ToUpperInvariant();
        var blocked = BlockedKeywords.FirstOrDefault(k => upperSql.Contains(k, StringComparison.Ordinal));

        if (blocked is not null)
        {
            return Task.FromResult(new AgentResult
            {
                AgentName = AgentName,
                Status = AgentStatus.Failed,
                ErrorMessage = $"Blocked unsafe SQL keyword: {blocked}"
            });
        }

        if (!upperSql.Contains($"TENANT_ID = '{context.TenantId}'".ToUpperInvariant()))
        {
            return Task.FromResult(new AgentResult
            {
                AgentName = AgentName,
                Status = AgentStatus.Failed,
                ErrorMessage = "SQL must include tenant_id filter."
            });
        }

        context.AgentOutputs[AgentName] = new { valid = true, sql = sqlResult.Sql };

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(new { valid = true }),
            NextAgentHint = "database.execution"
        });
    }
}
