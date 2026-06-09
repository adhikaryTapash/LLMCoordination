using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class ResponseComposerAgent : IAgent
{
    public const string AgentName = "response.composer";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var viewType = ResolveViewType(context);
        object composedData;

        if (context.AgentOutputs.TryGetValue(ApiExecutionAgent.AgentName, out var apiOutput) &&
            apiOutput is ToolExecutionResult apiResult)
        {
            composedData = viewType.Equals("table", StringComparison.OrdinalIgnoreCase)
                ? BuildTableFromPatients(apiResult.RawJson)
                : BuildCardsFromPatients(apiResult.RawJson);
        }
        else if (context.AgentOutputs.TryGetValue(DatabaseExecutionAgent.AgentName, out var dbOutput) &&
                 dbOutput is SqlExecutionResult dbResult)
        {
            composedData = new TableResponse
            {
                TotalRecords = dbResult.RowCount,
                Columns = [new TableColumn { Key = "count", Label = "Count" }],
                Rows = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(dbResult.RawJson) ?? []
            };
        }
        else if (context.AgentOutputs.TryGetValue(McpExecutionAgent.AgentName, out var mcpOutput) &&
                 mcpOutput is McpToolCallResult mcpResult)
        {
            composedData = JsonSerializer.Deserialize<object>(mcpResult.OutputJson) ?? new { message = mcpResult.OutputJson };
        }
        else
        {
            composedData = new { message = "No execution data available." };
        }

        context.AgentOutputs[AgentName] = composedData;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(composedData),
            NextAgentHint = "audit.telemetry"
        });
    }

    private static string ResolveViewType(AgentContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.ViewPreference))
        {
            return context.ViewPreference;
        }

        if (context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var nluObj) &&
            nluObj is NluAgentResponse nlu &&
            !string.IsNullOrWhiteSpace(nlu.Entities.ViewType))
        {
            return nlu.Entities.ViewType;
        }

        return "card";
    }

    private static CardResponse BuildCardsFromPatients(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var cards = new List<CardItem>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            cards.Add(new CardItem
            {
                Title = element.GetProperty("name").GetString() ?? "Unknown",
                Subtitle = $"MRN: {element.GetProperty("mrn").GetString()}",
                Fields =
                [
                    new CardField { Label = "Age", Value = element.GetProperty("age").GetInt32().ToString() },
                    new CardField { Label = "Gender", Value = element.GetProperty("gender").GetString() ?? "" },
                    new CardField { Label = "Mobile", Value = element.GetProperty("mobile").GetString() ?? "" }
                ],
                Actions =
                [
                    new CardAction
                    {
                        Label = "View Profile",
                        Action = "open_patient_profile",
                        EntityId = element.GetProperty("id").GetString() ?? ""
                    }
                ]
            });
        }

        return new CardResponse { TotalRecords = cards.Count, Cards = cards };
    }

    private static TableResponse BuildTableFromPatients(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var rows = new List<Dictionary<string, object>>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            rows.Add(new Dictionary<string, object>
            {
                ["id"] = element.GetProperty("id").GetString() ?? "",
                ["name"] = element.GetProperty("name").GetString() ?? "",
                ["mrn"] = element.GetProperty("mrn").GetString() ?? "",
                ["age"] = element.GetProperty("age").GetInt32(),
                ["gender"] = element.GetProperty("gender").GetString() ?? "",
                ["mobile"] = element.GetProperty("mobile").GetString() ?? ""
            });
        }

        return new TableResponse
        {
            TotalRecords = rows.Count,
            Columns =
            [
                new TableColumn { Key = "name", Label = "Name" },
                new TableColumn { Key = "mrn", Label = "MRN" },
                new TableColumn { Key = "age", Label = "Age" },
                new TableColumn { Key = "gender", Label = "Gender" },
                new TableColumn { Key = "mobile", Label = "Mobile" }
            ],
            Rows = rows
        };
    }
}
