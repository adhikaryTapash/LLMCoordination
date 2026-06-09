using LLMCoordination.Application.Agents;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Tests;

public class SqlValidationTests
{
    private readonly SqlValidationAgent _agent = new();

    [Fact]
    public async Task SqlValidation_BlocksDelete()
    {
        var context = CreateContext();
        context.AgentOutputs[SqlGenerationAgent.AgentName] = new SqlGenerationResult
        {
            Sql = "DELETE FROM patients WHERE tenant_id = '00000000-0000-0000-0000-000000000001'",
            Explanation = "unsafe",
            IsReadOnly = false
        };

        var result = await _agent.ExecuteAsync(context);

        Assert.Equal(AgentStatus.Failed, result.Status);
        Assert.Contains("DELETE", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SqlValidation_AllowsSelectWithLimit()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateContext(tenantId);
        context.AgentOutputs[SqlGenerationAgent.AgentName] = new SqlGenerationResult
        {
            Sql = $"SELECT * FROM patients WHERE tenant_id = '{tenantId}' LIMIT 20",
            Explanation = "safe",
            IsReadOnly = true
        };

        var result = await _agent.ExecuteAsync(context);

        Assert.Equal(AgentStatus.Success, result.Status);
    }

    [Fact]
    public async Task SqlGeneration_PatientList_ContainsSelectAndPatients()
    {
        var agent = new SqlGenerationAgent();
        var context = CreateContext();
        context.RawMessage = "list patients from database";
        context.AgentOutputs[NluIntentAgent.AgentName] = new NluAgentResponse
        {
            Intent = new IntentResult
            {
                Name = "database.query.count",
                Resource = "patients",
                Action = "read"
            },
            Entities = new EntityResult { Operation = "list", Resource = "patients" }
        };

        await agent.ExecuteAsync(context);

        var sql = Assert.IsType<SqlGenerationResult>(context.AgentOutputs[SqlGenerationAgent.AgentName]);
        Assert.Contains("SELECT", sql.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("patients", sql.Sql, StringComparison.OrdinalIgnoreCase);
    }

    private static AgentContext CreateContext(Guid? tenantId = null) =>
        new()
        {
            TenantId = tenantId ?? Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserRole = "Admin",
            ConversationId = Guid.NewGuid()
        };
}
