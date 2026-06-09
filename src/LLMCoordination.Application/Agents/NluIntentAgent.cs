using System.Text.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class NluIntentAgent : IAgent
{
    public const string AgentName = "nlu.intent";

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var message = context.RawMessage.ToLowerInvariant();
        var response = new NluAgentResponse();

        if (ContainsDatabaseQuery(message))
        {
            response.Intent = new IntentResult
            {
                Name = "database.query.count",
                Confidence = 0.85,
                Domain = "database",
                Resource = "database",
                Action = "read"
            };
            response.Entities = new EntityResult
            {
                Resource = "database",
                Operation = "count",
                ViewType = context.ViewPreference
            };
            response.RoutingHints = new RoutingHints
            {
                RequiresDatabase = true,
                RequiresSwagger = false,
                SuggestedSkill = "database.sql.generate",
                SuggestedNextAgent = "database.schema.agent"
            };
        }
        else if (message.Contains("appointment"))
        {
            response.Intent = new IntentResult
            {
                Name = "appointment.list.upcoming",
                Confidence = 0.92,
                Domain = "healthcare",
                Resource = "appointment",
                Action = "read"
            };
            response.Entities = new EntityResult
            {
                Resource = "appointments",
                Operation = "list",
                TimeScope = "upcoming",
                ViewType = context.ViewPreference,
                Limit = 20
            };
            response.RoutingHints = new RoutingHints
            {
                RequiresPatientResolution = true,
                RequiresSwagger = true,
                SuggestedSkill = "api.query.execute",
                SuggestedNextAgent = "tenant.context.agent"
            };
        }
        else if (message.Contains("patient") && (message.Contains("list") || message.Contains("show") || message.Contains("give")))
        {
            response.Intent = new IntentResult
            {
                Name = "patient.list",
                Confidence = 0.95,
                Domain = "healthcare",
                Resource = "patient",
                Action = "read"
            };
            response.Entities = new EntityResult
            {
                Resource = "patients",
                Operation = "list",
                ViewType = string.IsNullOrWhiteSpace(context.ViewPreference) ? "list" : context.ViewPreference,
                Limit = 20
            };
            response.RoutingHints = new RoutingHints
            {
                RequiresSwagger = true,
                SuggestedSkill = "api.query.execute",
                SuggestedNextAgent = "tenant.context.agent"
            };
        }
        else
        {
            response.Intent = new IntentResult
            {
                Name = "general.inquiry",
                Confidence = 0.5,
                Domain = "general",
                Resource = "unknown",
                Action = "read"
            };
            response.Entities.ViewType = context.ViewPreference;
        }

        context.AgentOutputs[AgentName] = response;

        return Task.FromResult(new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(response),
            NextAgentHint = response.RoutingHints.SuggestedNextAgent
        });
    }

    private static bool ContainsDatabaseQuery(string message) =>
        (message.Contains("database") || message.Contains("db")) &&
        (message.Contains("count") || message.Contains("query") || message.Contains("sql"));
}
