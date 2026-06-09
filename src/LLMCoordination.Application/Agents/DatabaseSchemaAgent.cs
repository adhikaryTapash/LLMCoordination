using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class DatabaseSchemaAgent : IAgent
{
    public const string AgentName = "database.schema";

    private readonly IDatabaseRepository _databaseRepository;

    public DatabaseSchemaAgent(IDatabaseRepository databaseRepository)
    {
        _databaseRepository = databaseRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var schema = await _databaseRepository.GetSchemaAsync(context.TenantId, cancellationToken: cancellationToken);
        var tables = schema
            .GroupBy(s => s.TableName)
            .Select(g => new SchemaTableDto
            {
                TableName = g.Key,
                Columns = g.Select(c => new SchemaColumnDto
                {
                    ColumnName = c.ColumnName,
                    DataType = c.DataType,
                    IsNullable = c.IsNullable
                }).ToList()
            })
            .ToList();

        if (tables.Count == 0)
        {
            tables.Add(new SchemaTableDto
            {
                TableName = "patients",
                Columns =
                [
                    new SchemaColumnDto { ColumnName = "id", DataType = "uuid", IsNullable = false },
                    new SchemaColumnDto { ColumnName = "name", DataType = "varchar", IsNullable = false },
                    new SchemaColumnDto { ColumnName = "mrn", DataType = "varchar", IsNullable = false }
                ]
            });
        }

        context.AgentOutputs[AgentName] = tables;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(tables),
            NextAgentHint = "sql.generation"
        };
    }
}
