using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Entities;

namespace LLMCoordination.Application.Services;

public class DbSchemaReaderService
{
    private readonly IDatabaseRepository _databaseRepository;

    public DbSchemaReaderService(IDatabaseRepository databaseRepository)
    {
        _databaseRepository = databaseRepository;
    }

    public async Task<DatabaseConnectionDto> RegisterConnectionAsync(
        Guid tenantId,
        RegisterDatabaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var connection = new TenantDatabaseConnection
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DbType = request.DbType,
            Host = request.Host,
            DatabaseName = request.DatabaseName,
            EncryptedConnectionString = request.ConnectionString,
            ReadOnly = request.ReadOnly,
            Status = "Active"
        };

        await _databaseRepository.AddConnectionAsync(connection, cancellationToken);

        var schema = GetMockSchema(tenantId, connection.Id, request.DbType);
        await _databaseRepository.AddSchemaEntriesAsync(schema, cancellationToken);

        return new DatabaseConnectionDto
        {
            Id = connection.Id,
            DbType = connection.DbType,
            Host = connection.Host,
            DatabaseName = connection.DatabaseName,
            ReadOnly = connection.ReadOnly,
            Status = connection.Status
        };
    }

    public async Task<IReadOnlyList<SchemaTableDto>> GetSchemaTablesAsync(
        Guid tenantId,
        Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        var schema = await _databaseRepository.GetSchemaAsync(tenantId, connectionId, cancellationToken);
        return schema
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
    }

    private static List<TenantDatabaseSchema> GetMockSchema(Guid tenantId, Guid connectionId, string dbType)
    {
        var tables = dbType.ToLowerInvariant() switch
        {
            "postgres" or "postgresql" => new[] { "patients", "appointments", "providers" },
            "sqlserver" or "mssql" => new[] { "Patients", "Appointments", "Providers" },
            _ => new[] { "patients", "appointments" }
        };

        var entries = new List<TenantDatabaseSchema>();
        foreach (var table in tables)
        {
            entries.Add(new TenantDatabaseSchema
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connectionId,
                TableName = table,
                ColumnName = "id",
                DataType = "uuid",
                IsNullable = false
            });
            entries.Add(new TenantDatabaseSchema
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connectionId,
                TableName = table,
                ColumnName = "tenant_id",
                DataType = "uuid",
                IsNullable = false
            });
            entries.Add(new TenantDatabaseSchema
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connectionId,
                TableName = table,
                ColumnName = "name",
                DataType = "varchar",
                IsNullable = true
            });
        }

        return entries;
    }
}
