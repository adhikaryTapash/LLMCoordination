using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class ApiExecutionAgent : IAgent
{
    public const string AgentName = "api.execution";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var patients = new[]
        {
            new { id = "1001", name = "Rahul Sharma", mrn = "P-1001", age = 42, gender = "Male", mobile = "9876543210" },
            new { id = "1002", name = "Priya Patel", mrn = "P-1002", age = 35, gender = "Female", mobile = "9876543211" },
            new { id = "1003", name = "Amit Kumar", mrn = "P-1003", age = 28, gender = "Male", mobile = "9876543212" }
        };

        var result = new ToolExecutionResult
        {
            Status = "success",
            RawJson = JsonSerializer.Serialize(patients),
            RecordCount = patients.Length
        };

        context.AgentOutputs[AgentName] = result;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = result.RawJson,
            NextAgentHint = "response.composer"
        });
    }
}
